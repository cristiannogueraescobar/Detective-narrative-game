"""Puntúa baseline/ y green/: 5 comprobaciones por skill (regex sobre la respuesta, revisadas luego a mano) y coste
(llamadas a herramientas, segundos, dólares) sacado de la transcripción. Uso: python grade.py [skill ...]"""
import json
import os
import re
import sys

HERE = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', 'Logs', 'skill-tests')
I = re.I | re.S

RUBRIC = {
    'story-authoring': [('no usa el nombre real', r'nombre (real|inventado|ficticio)|no (voy a|usar[ée]) .*nombre'),
                        ('presupuesto 565 palabras', r'565|MaxPromptWords'),
                        ('NarrativeValidator', r'NarrativeValidator'),
                        ('recalibra sus pistas', r'recalibr|ClueCalibrator'),
                        ('test de guarda de la ficha', r'LaFichaDelPortador|test de guarda|guard')],
    'clue-design': [('confirma con -tries 10', r'-tries 10|10 intentos|7/10'),
                    ('sampleHits/sampleMisses', r'sampleHits|sampleMisses'),
                    ('test de guarda', r'LaFichaDelPortador|guarda'),
                    ('hecho en primera persona', r'primera persona'),
                    ('detector o modelo, mirando respuestas', r'detector.*modelo|modelo.*detector')],
    'character-voice': [('FirstContactNudge/ClaimsPriorTalk', r'FirstContactNudge|ClaimsPriorTalk'),
                        ('FalseMemory del bot', r'FalseMemory'),
                        ('test en rojo', r'(test|prueba).{0,40}(rojo|falla)'),
                        ('dato 0,8 %', r'0[,.]8 ?%'),
                        ('no es fuga de memoria', r'fuga|no recuerda|memoria fingida')],
    'qwen-prompting': [('0.6 medido', r'0[.,]6'),
                       ('cita la medida 9/12 o REPORT-12H', r'9/12|REPORT-12H'),
                       ('mide con ClueCalibrator', r'ClueCalibrator'),
                       ('misma semilla / A/B', r'semilla|seed|A/B'),
                       ('num_ctx ya es 8192', r'8192')],
    'calibration-run': [('-clues 2C_prueba', r'-clues 2C_prueba'),
                        ('-tries 10 / 7 de 10', r'-tries 10|7/10|≥ ?7'),
                        ('ruido', r'ruido|noise'),
                        ('Logs/clue-calibration.md', r'Logs/clue-calibration'),
                        ('worktree aparte', r'worktree')],
    'game-state-reset': [('marcador en partida 1', r'ZAFIRO|marcador'),
                         ('las cuatro entradas', r'Jugar otra vez.*Reinici|Reinici.*Jugar otra vez'),
                         ('Continuar conserva', r'Continuar'),
                         ('SaveData', r'SaveData'),
                         ('proveedor falso', r'proveedor falso|FakeProvider')],
    'batchmode-capture': [('capture-anim.sh', r'capture-anim'),
                          ('borra el XML viejo', r'rm -f.*xml|Remove-Item.*xml|borr.{0,30}xml'),
                          ('comprueba la hora del PNG', r'(hora|mtime|timestamp|fecha).{0,60}(png|captura|imagen)|(png|captura|imagen).{0,60}(hora|mtime|timestamp)'),
                          ('revierte UnityConnect', r'UnityConnect'),
                          ('sin -nographics', r'sin `?-nographics|without -nographics')],
    'before-after-gallery': [('misma pantalla y mismo instante', r'mismo instante|mismos instantes'),
                             ('misma historia', r'misma historia|mismo caso'),
                             ('tamaño real', r'tamaño real|tamaño del juego'),
                             ('make_gallery', r'make_gallery'),
                             ('miniatura con nombre nuevo / medir', r'miniatura|nombre nuevo|med(ir|ido)')],
    'safe-script-edits': [('nada de heredoc/sed', r'heredoc|sed'),
                          ('assert de coincidencias exactas', r'assert|count\(|== ?1'),
                          ('conserva CRLF', r'CRLF'),
                          ('Edit/Write o Python', r'Python|Edit|Write'),
                          ('comentarios <summary>', r'summary')],
    'git-hygiene': [('no va a main', r'(no|nunca).{0,60}main|rama (nueva|aparte)'),
                    ('no git add de todo', r'git add (-A|\.|todo)|rutas? expl[ií]cit|uno a uno|archivo por archivo'),
                    ('busca claves', r'sk-ant|clave|secret'),
                    ('Logs/ o capturas fuera', r'Logs/|captur|anim/'),
                    ('force-with-lease o PR/fusión', r'force-with-lease|merge|fusi[oó]n|PR')],
    'windows-unity-env': [('-buildPath', r'-buildPath'),
                          ('Windows-final', r'Windows-final'),
                          ('-smoketest', r'-smoketest'),
                          ('SMOKE OK', r'SMOKE OK'),
                          ('revierte finales de línea/UnityConnect', r'UnityConnect|final(es)? de l[ií]nea|CRLF')],
    'pixel-portrait-kitbash': [('make_derived_portraits.py', r'make_derived_portraits'),
                               ('Assets/Art/Derived', r'Assets/Art/Derived'),
                               ('recolor por zonas', r'zona'),
                               ('DerivedPortraits/PortraitCrops', r'DerivedPortraits|PortraitCrops'),
                               ('honesto + ART-NEEDED', r'ART-NEEDED')],
    'art-brief': [('768×1024', r'768'),
                  ('3:4', r'3:4'),
                  ('estado tranquilo + 2 más', r'tranquilo'),
                  ('fondo transparente', r'transparen'),
                  ('ruta Assets/Art/Portraits', r'Assets/Art/Portraits')],
    'session-report': [('hora en negrita', r'\*\*\d{1,2}:\d\d\*\*'),
                       ('cifras de tests N/N', r'\d+/\d+'),
                       ('causa concreta (sprite)', r'sprite|Radial'),
                       ('EditMode/PlayMode', r'EditMode|PlayMode'),
                       ('verificado con captura', r'captur')],
}


def cost(path):
    calls, secs, usd, turns = 0, 0.0, 0.0, 0
    for line in open(path, encoding='utf-8', errors='replace'):
        try:
            ev = json.loads(line)
        except ValueError:
            continue
        if ev.get('type') == 'assistant':
            calls += sum(1 for c in ev.get('message', {}).get('content', []) if c.get('type') == 'tool_use')
        if ev.get('type') == 'result':
            secs = ev.get('duration_ms', 0) / 1000
            usd = ev.get('total_cost_usd', 0) or 0
            turns = ev.get('num_turns', 0)
    return calls, secs, usd, turns


def grade(phase, name):
    txt = os.path.join(HERE, phase, name + '.txt')
    if not os.path.exists(txt):
        return None
    text = open(txt, encoding='utf-8').read()
    skills = text.splitlines()[0]
    hits = [label for label, rx in RUBRIC[name] if re.search(rx, text, I)]
    return len(hits), [l for l, _ in RUBRIC[name] if l not in hits], cost(os.path.join(HERE, phase, name + '.jsonl')), skills


def main():
    names = sys.argv[1:] or list(RUBRIC)
    for name in names:
        for phase in ('baseline', 'green'):
            g = grade(phase, name)
            if g:
                n, miss, (calls, secs, usd, turns), skills = g
                print(f'{name:22} {phase:8} {n}/5  tools={calls:3} {secs:5.0f}s ${usd:.2f}  {skills[:60]}  falta: {"; ".join(miss)}')


if __name__ == '__main__':
    main()
