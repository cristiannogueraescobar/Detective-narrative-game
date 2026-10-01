"""Corre cada tarea simulada en un proceso nuevo de claude -p (sin poder modificar nada) y guarda:
<fase>/<skill>.jsonl (transcripción) y <fase>/<skill>.txt (skills invocadas + respuesta final).
Uso: python Tools/skill-tests/run.py <fase> [skill ...]   (fase: baseline sin .claude/skills, green con ellas).
Cada tarea tienta el error que su skill evita. Ver docs/SKILLS-SETUP.md."""
import concurrent.futures as cf
import json
import os
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
PROJECT = os.path.abspath(os.path.join(HERE, '..', '..'))
OUT = os.path.join(PROJECT, 'Logs', 'skill-tests')  # Resultados fuera del repositorio
SUFFIX = ('\n\n(Simulación: NO ejecutes comandos ni modifiques archivos. Puedes leer el proyecto. '
          'Responde con lo que harías, concreto: archivos, comandos, comprobaciones.)')


def run(phase, name, task):
    out = os.path.join(OUT, phase)
    os.makedirs(out, exist_ok=True)
    cmd = ['claude', '-p', task + SUFFIX, '--output-format', 'stream-json', '--verbose', '--max-turns', '12',
           '--disallowedTools', 'Bash', 'PowerShell', 'Edit', 'Write', 'NotebookEdit']
    p = subprocess.run(cmd, cwd=PROJECT, stdin=subprocess.DEVNULL, capture_output=True, text=True,
                       encoding='utf-8', errors='replace', timeout=900)
    with open(os.path.join(out, name + '.jsonl'), 'w', encoding='utf-8') as f:
        f.write(p.stdout)
    skills, result = [], ''
    for line in p.stdout.splitlines():
        try:
            ev = json.loads(line)
        except ValueError:
            continue
        if ev.get('type') == 'assistant':
            for c in ev.get('message', {}).get('content', []):
                if c.get('type') == 'tool_use' and c.get('name') == 'Skill':
                    skills.append(c.get('input', {}).get('skill'))
        if ev.get('type') == 'result':
            result = ev.get('result', '')
    with open(os.path.join(out, name + '.txt'), 'w', encoding='utf-8') as f:
        f.write('SKILLS: ' + ', '.join(s for s in skills if s) + '\n\n' + result)
    return name, skills, len(result)


def main():
    phase = sys.argv[1]
    tasks = json.load(open(os.path.join(HERE, 'tasks.json'), encoding='utf-8'))
    names = sys.argv[2:] or list(tasks)
    with cf.ThreadPoolExecutor(4) as pool:
        for name, skills, n in pool.map(lambda k: run(phase, k, tasks[k]), names):
            print(f'{name:22} skills={skills} chars={n}', flush=True)


if __name__ == '__main__':
    main()
