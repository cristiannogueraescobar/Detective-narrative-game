"""Audio PROVISIONAL sintetizado por código (archivos nuevos en Assets/Resources/Audio/). Sustitúyelo por el
definitivo con el mismo nombre (ver AUDIO-NEEDED.md). Todo es discreto a propósito: mejor casi no notarlo que
molestar. Uso: python Tools/make_placeholder_audio.py
"""
import os

import numpy as np
from scipy.io import wavfile
from scipy.signal import butter, lfilter

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'Assets', 'Resources', 'Audio')
SR = 44100
RNG = np.random.default_rng(7)


def t(seconds, sr=SR):
    return np.arange(int(seconds * sr)) / sr


def env(n, attack=0.005, decay=0.2, sr=SR):
    x = np.arange(n) / sr
    a = np.clip(x / max(attack, 1e-4), 0, 1)
    return a * np.exp(-np.maximum(x - attack, 0) / max(decay, 1e-4))


def band(sig, lo, hi, sr=SR):
    b, a = butter(2, [lo / (sr / 2), min(hi / (sr / 2), 0.99)], btype='band')
    return lfilter(b, a, sig)


def low(sig, cut, sr=SR):
    b, a = butter(2, cut / (sr / 2), btype='low')
    return lfilter(b, a, sig)


def noise(seconds, sr=SR):
    return RNG.standard_normal(int(seconds * sr))


def tone(freq, seconds, harmonics=(1.0,), sr=SR):
    x = t(seconds, sr)
    return sum(a * np.sin(2 * np.pi * freq * (i + 1) * x) for i, a in enumerate(harmonics))


def save(path, sig, peak=0.6, sr=SR, fade_out=True):
    sig = np.asarray(sig, dtype=np.float64)
    m = np.max(np.abs(sig)) or 1.0
    sig = sig / m * peak
    fade = min(len(sig) // 20, int(0.01 * sr))
    if fade > 0 and fade_out:  # Los bucles no: el cruce ya empalma el final con el principio
        sig[-fade:] *= np.linspace(1, 0, fade)
    full = os.path.join(ROOT, path + '.wav')
    os.makedirs(os.path.dirname(full), exist_ok=True)
    wavfile.write(full, sr, (sig * 32767).astype(np.int16))
    return full


def pad(sig, before=0.005):
    return np.concatenate([np.zeros(int(before * SR)), sig])


made = []

# --- Efectos ---
# Tecla de máquina: clic seco
key = band(noise(0.05), 1500, 6000) * env(int(0.05 * SR), 0.001, 0.008)
key += 0.3 * tone(180, 0.05) * env(int(0.05 * SR), 0.001, 0.01)
made.append(save('sfx/clic', pad(key), 0.35))
made.append(save('sfx/maquina', pad(band(noise(0.04), 2000, 7000) * env(int(0.04 * SR), 0.001, 0.006)), 0.3))

# Enviar: carraca del carro y un tope
n = int(0.32 * SR)
ratchet = np.zeros(n)
for k, pos in enumerate(np.linspace(0, 0.22, 9) ** 1.2):
    i = int(pos * SR)
    c = band(noise(0.02), 2500, 8000) * env(int(0.02 * SR), 0.0005, 0.004)
    ratchet[i:i + len(c)] += c * (0.5 + 0.05 * k)
stop = low(noise(0.08), 400) * env(int(0.08 * SR), 0.001, 0.03)
ratchet[int(0.24 * SR):int(0.24 * SR) + len(stop)] += stop * 1.2
made.append(save('sfx/enviar', pad(ratchet), 0.4))

# Respuesta: papel que se desliza
paper = band(noise(0.35), 800, 5000) * np.sin(np.linspace(0, np.pi, int(0.35 * SR))) ** 2
made.append(save('sfx/respuesta', pad(paper), 0.25))

# Sello: golpe de goma sobre madera
thud = low(noise(0.3), 300) * env(int(0.3 * SR), 0.001, 0.05) + 0.8 * tone(85, 0.3) * env(int(0.3 * SR), 0.001, 0.06)
made.append(save('sfx/sello', pad(thud), 0.7))

# Pista: ficha que cae + vibráfono suave (La mayor con novena)
seconds = 1.4
card = low(noise(0.1), 900) * env(int(0.1 * SR), 0.001, 0.02)
vib = np.zeros(int(seconds * SR))
for k, f in enumerate([220.0, 277.18, 329.63, 493.88]):
    note = tone(f, seconds - 0.1 * k, (1.0, 0.0, 0.25, 0.0, 0.08)) * env(int((seconds - 0.1 * k) * SR), 0.004, 0.55)
    note *= 1 + 0.15 * np.sin(2 * np.pi * 5.5 * t(seconds - 0.1 * k))
    start = int((0.08 + 0.06 * k) * SR)
    vib[start:start + len(note)] += note[:len(vib) - start]
vib[:len(card)] += card * 2
made.append(save('sfx/pista', pad(vib), 0.45))

# Contradicción: golpe de cuerdas graves (cluster La-Sib) con cola
seconds = 1.6
x = t(seconds)
strings = sum(np.sign(np.sin(2 * np.pi * f * x)) * 0.5 + np.sin(2 * np.pi * f * 2 * x) * 0.3 for f in [110.0, 116.54, 110.6])
strings = low(strings, 900) * env(len(x), 0.02, 0.6)
strings += 0.6 * low(noise(seconds), 150) * env(len(x), 0.001, 0.08)
made.append(save('sfx/contradiccion', pad(strings), 0.55))

# Nuevo día: hoja arrancada + campanada lejana
tear = band(noise(0.55), 1200, 7000) * (0.6 + 0.4 * np.abs(RNG.standard_normal(int(0.55 * SR)))) * np.sin(np.linspace(0, np.pi, int(0.55 * SR)))
bell = tone(659.25, 2.0, (1.0, 0.0, 0.5, 0.0, 0.3, 0.0, 0.2)) * env(int(2.0 * SR), 0.003, 0.7)
day = np.zeros(int(2.4 * SR))
day[:len(tear)] += tear * 0.6
day[int(0.4 * SR):int(0.4 * SR) + len(bell)] += bell * 0.35
made.append(save('sfx/nuevo_dia', pad(day), 0.4))

# Acusación: dos golpes de mazo y sala
gavel = np.zeros(int(2.4 * SR))
for s0 in (0.0, 0.35):
    k = low(noise(0.4), 500) * env(int(0.4 * SR), 0.001, 0.06) + tone(110, 0.4) * env(int(0.4 * SR), 0.001, 0.05)
    gavel[int(s0 * SR):int(s0 * SR) + len(k)] += k
gavel += 0.05 * low(noise(2.4), 200) * np.linspace(1, 0, int(2.4 * SR))
made.append(save('sfx/acusacion', pad(gavel), 0.6))

# Latido (lub-dub)
beat = np.zeros(int(0.8 * SR))
for s0, f, a in ((0.0, 55.0, 1.0), (0.22, 48.0, 0.7)):
    x = t(0.18)
    thump = np.sin(2 * np.pi * (f * x - 20 * x * x)) * env(len(x), 0.004, 0.05) * a
    beat[int(s0 * SR):int(s0 * SR) + len(thump)] += thump
made.append(save('sfx/latido', pad(low(beat, 200)), 0.7))


def chord(freqs, seconds, arpeggio=0.12, decay=1.4):
    out = np.zeros(int(seconds * SR))
    for k, f in enumerate(freqs):
        dur = seconds - arpeggio * k
        n2 = tone(f, dur, (1.0, 0.45, 0.2, 0.1)) * env(int(dur * SR), 0.005, decay)
        s0 = int(arpeggio * k * SR)
        out[s0:s0 + len(n2)] += n2
    return low(out, 3000)


made.append(save('sfx/final_bueno', pad(chord([130.81, 196.0, 261.63, 329.63, 392.0], 4.0)), 0.5))           # Do mayor
made.append(save('sfx/final_agridulce', pad(chord([130.81, 196.0, 261.63, 349.23, 392.0], 4.0)), 0.5))       # Do sus4: sin resolver
made.append(save('sfx/final_insuficiente', pad(chord([110.0, 164.81, 220.0, 261.63, 329.63], 4.5, 0.2, 1.8)), 0.5))  # La menor, lento
bad = chord([98.0, 103.83, 146.83, 155.56], 4.5, 0.05, 2.0) + 0.15 * band(noise(4.5), 1000, 6000)[: int(4.5 * SR)]
made.append(save('sfx/final_malo', pad(bad), 0.5))                                                          # Cluster disonante y lluvia

# --- Música (bucles de 30 s, 22 kHz): ambientes discretos, no melodías ---
MSR = 22050


def loop(sig, sr=MSR, cross=1.5):
    """Cierra el bucle mezclando el final con el principio."""
    c = int(cross * sr)
    body = sig[:-c].copy()
    body[:c] = body[:c] * np.linspace(0, 1, c) + sig[-c:] * np.linspace(1, 0, c)
    return body


def drone(freqs, seconds, sr=MSR, lfo=0.07):
    x = t(seconds, sr)
    out = sum(np.sin(2 * np.pi * f * x + np.sin(2 * np.pi * 0.11 * x) * 0.4) / (i + 1) for i, f in enumerate(freqs))
    return out * (0.7 + 0.3 * np.sin(2 * np.pi * lfo * x))


def rain(seconds, sr=MSR, density=18):
    base = low(RNG.standard_normal(int(seconds * sr)), 2500, sr) * 0.5
    drops = np.zeros_like(base)
    for i in RNG.integers(0, len(base) - 400, int(seconds * density)):
        drops[i:i + 300] += band(RNG.standard_normal(300), 2000, 8000, sr) * np.exp(-np.arange(300) / 40)
    return base + drops * 0.4


def hz(note):
    """'A4' -> 440; admite sostenidos ('G#4') y bemoles ('Bb3')."""
    names = {'C': -9, 'D': -7, 'E': -5, 'F': -4, 'G': -2, 'A': 0, 'B': 2}
    semis = names[note[0]] + (1 if '#' in note else -1 if 'b' in note[1:-1] else 0)
    return 440.0 * 2 ** ((semis + 12 * (int(note[-1]) - 4)) / 12)


def piano(freq, seconds, sr=MSR, rng=None):
    """Nota de piano aditiva: parciales algo inarmónicos que se apagan antes cuanto más agudos, y martillo."""
    x = t(seconds, sr)
    out = np.zeros_like(x)
    for k in range(1, 7):
        f = freq * k * np.sqrt(1 + 0.0004 * k * k)
        if f > sr / 2 - 500:
            break
        out += np.sin(2 * np.pi * f * x) * (0.6 ** (k - 1)) * np.exp(-x * (1.2 + 0.9 * k))
    hammer = rng.standard_normal(len(x)) * np.exp(-x * 90) * 0.05
    return (out + hammer) * np.clip(x / 0.004, 0, 1)


def bass(freq, seconds, sr=MSR):
    """Contrabajo pizzicato: fundamental con algo de segundo armónico, ataque seco y cola corta."""
    x = t(seconds, sr)
    s = np.sin(2 * np.pi * freq * x) + 0.35 * np.sin(2 * np.pi * 2 * freq * x) + 0.1 * np.sin(2 * np.pi * 3 * freq * x)
    return low(s * np.exp(-x * 2.6) * np.clip(x / 0.008, 0, 1), 900, sr)


def noir_motif(bars=8, bpm=64, sr=MSR, seed=11):
    """Motivo noir en La menor (Am | Am | Fmaj7 | Fmaj7 | Dm6 | Dm6 | E7b9 | E7b9): piano escaso, contrabajo en
    1 y 3, escobillas muy bajas en 2 y 4. Dura exactamente bars compases, para que el bucle no pierda el pulso."""
    rng = np.random.default_rng(seed)
    beat = 60.0 / bpm
    bar = 4 * beat
    out = np.zeros(int(bars * bar * sr) + sr * 4)

    def put(sig, at):
        i = int(at * sr)
        out[i:i + len(sig)] += sig[:max(0, len(out) - i)]

    chords = [['A2', 'E3', 'B3', 'C4'], ['A2', 'E3', 'B3', 'C4'], ['F2', 'C3', 'E3', 'A3'], ['F2', 'C3', 'E3', 'A3'],
              ['D2', 'A2', 'F3', 'B3'], ['D2', 'A2', 'F3', 'B3'], ['E2', 'G#3', 'D4', 'F4'], ['E2', 'G#3', 'D4', 'F4']]
    roots = [('A1', 'E2'), ('A1', 'C2'), ('F1', 'C2'), ('F1', 'A1'), ('D2', 'A1'), ('D2', 'F2'), ('E2', 'B1'), ('E2', 'G#1')]
    # Melodía: (compás, pulso, nota, pulsos). Mucho silencio: se oye la lluvia entre frases
    melody = [(0, 0, 'E5', 1.5), (0, 1.5, 'C5', 0.5), (0, 2, 'B4', 2), (1, 0, 'A4', 3),
              (2, 1, 'C5', 1), (2, 2, 'E5', 1), (2, 3, 'A5', 1), (3, 0, 'G5', 3.5),
              (4, 0, 'F5', 1.5), (4, 1.5, 'E5', 0.5), (4, 2, 'D5', 2), (5, 0, 'B4', 3),
              (6, 0, 'G#4', 1), (6, 1, 'B4', 1), (6, 2, 'D5', 1), (6, 3, 'F5', 1), (7, 0, 'E5', 3.5)]

    for b in range(bars):
        start = b * bar
        # Acorde apagado a contratiempo (el piano de la mano izquierda), ligeramente arpegiado
        for k, n in enumerate(chords[b][1:]):
            put(piano(hz(n), bar, sr, rng) * 0.16, start + beat * 0.5 + k * 0.03)
        for i, n in enumerate(roots[b]):
            put(bass(hz(n), beat * 1.9, sr) * 0.55, start + i * 2 * beat)
        for p in (1, 3):
            swish = band(rng.standard_normal(int(beat * 0.6 * sr)), 2500, 7000, sr)
            put(swish * np.exp(-np.arange(len(swish)) / (sr * 0.12)) * 0.02, start + p * beat)
    for b, p, n, d in melody:
        put(piano(hz(n), d * beat + 1.2, sr, rng) * 0.3, b * bar + p * beat)

    # Cola: lo que suena después del último compás vuelve al principio (bucle sin corte)
    body_len = int(bars * bar * sr)
    body = out[:body_len].copy()
    tail = out[body_len:]
    body[:len(tail)] += tail
    return body


seconds = 31.5
menu_bed = loop(rain(seconds) * 0.8 + drone([55.0, 82.41, 110.0], seconds) * 0.25)  # 30 s exactos (31,5 − 1,5)
motif = noir_motif()
menu_bed = menu_bed / np.max(np.abs(menu_bed)) * 0.55 + motif[:len(menu_bed)] / np.max(np.abs(motif)) * 0.45
made.append(save('musica/menu', menu_bed, 0.35, MSR, fade_out=False))
# --- Motivos de cada historia (día 3): escasos y bajos, porque suenan mientras se interroga. Cada uno con su RNG
# para no cambiar el ruido de los ambientes. 8 compases a 64 ppm = 30 s exactos (el bucle no pierde el pulso).

def organ(freq, seconds, sr=MSR):
    """Lengüeta suave (tipo acordeón): impares fuertes, ataque lento y un vibrato leve."""
    x = t(seconds, sr)
    vib = 1 + 0.004 * np.sin(2 * np.pi * 5.2 * x)
    s = sum(np.sin(2 * np.pi * freq * k * vib * x) * a for k, a in ((1, 1.0), (2, 0.35), (3, 0.5), (5, 0.18)))
    return low(s * np.clip(x / 0.12, 0, 1) * np.clip((seconds - x) / 0.25, 0, 1), 2500, sr)


def pluck(freq, seconds, sr=MSR, rng=None, bright=0.5):
    """Cuerda punteada (Karplus-Strong): guitarra seca."""
    n = int(seconds * sr)
    period = max(2, int(sr / freq))
    buf = rng.uniform(-1, 1, period)
    out = np.zeros(n)
    for i in range(n):
        out[i] = buf[i % period]
        nxt = (i + 1) % period
        buf[i % period] = 0.996 * (bright * buf[i % period] + (1 - bright) * buf[nxt])
    return low(out, 3500, sr)


def score(bars, bar, events, sr=MSR):
    """events: (inicio en s, señal). Devuelve exactamente bars*bar segundos, con la cola vuelta al principio."""
    out = np.zeros(int(bars * bar * sr) + sr * 5)
    for at, sig in events:
        i = int(at * sr)
        out[i:i + len(sig)] += sig[:max(0, len(out) - i)]
    body_len = int(bars * bar * sr)
    body = out[:body_len].copy()
    tail = out[body_len:]
    body[:len(tail)] += tail
    return body


BEAT = 60.0 / 64
BAR = 4 * BEAT


def motif_story1(seed=21):
    """La hija perfecta: casa en Santiago, lluvia. Piano solo en Re menor, muy escaso, y un reloj de pared."""
    rng = np.random.default_rng(seed)
    ev = []
    chords = [['D3', 'A3', 'F4'], ['D3', 'A3', 'F4'], ['Bb2', 'F3', 'D4'], ['A2', 'E3', 'C#4'],
              ['D3', 'A3', 'F4'], ['G2', 'D3', 'Bb3'], ['Bb2', 'F3', 'D4'], ['A2', 'E3', 'C#4']]
    for b, ch in enumerate(chords):
        for k, n in enumerate(ch):
            ev.append((b * BAR + k * 0.05, piano(hz(n), BAR + 1.0, MSR, rng) * 0.14))
    melody = [(0, 0, 'D5', 2), (0, 2, 'A4', 2), (1, 0, 'F4', 3.5), (3, 0, 'E5', 1), (3, 1, 'D5', 1), (3, 2, 'C#5', 2),
              (4, 0, 'D5', 2), (4, 2, 'F5', 2), (5, 0, 'E5', 3.5), (7, 0, 'A4', 3.5)]
    for b, p, n, d in melody:
        ev.append((b * BAR + p * BEAT, piano(hz(n), d * BEAT + 1.5, MSR, rng) * 0.26))
    tick = band(rng.standard_normal(int(0.03 * MSR)), 1800, 5000, MSR) * np.exp(-np.arange(int(0.03 * MSR)) / (MSR * 0.004))
    for k in range(int(8 * 4)):
        ev.append((k * BEAT, tick * (0.05 if k % 2 == 0 else 0.035)))  # Tic, tac
    return score(8, BAR, ev)


def motif_story2(seed=22):
    """Noche de verano: costa gallega de madrugada. Frase lenta en Mi eolio, de lengüeta, como una gaita lejana."""
    ev = []
    for b, n in enumerate(['E3', 'E3', 'C3', 'D3', 'E3', 'E3', 'C3', 'B2']):
        ev.append((b * BAR, organ(hz(n), BAR, MSR) * 0.12))
    melody = [(0, 0, 'E5', 1.5), (0, 1.5, 'F#5', 0.5), (0, 2, 'G5', 2), (1, 0, 'F#5', 1), (1, 1, 'E5', 3),
              (2, 0, 'D5', 2), (2, 2, 'E5', 2), (3, 0, 'B4', 4),
              (4, 0, 'E5', 1.5), (4, 1.5, 'G5', 0.5), (4, 2, 'A5', 2), (5, 0, 'G5', 1), (5, 1, 'F#5', 3),
              (6, 0, 'E5', 2), (6, 2, 'D5', 2), (7, 0, 'E5', 4)]
    for b, p, n, d in melody:
        ev.append((b * BAR + p * BEAT, organ(hz(n), d * BEAT, MSR) * 0.16))
    return score(8, BAR, ev)


def motif_story3(seed=23):
    """Humo y silencio: olivar en Jaén. Guitarra punteada con la cadencia andaluza (Lam-Sol-Fa-Mi), sin prisa."""
    rng = np.random.default_rng(seed)
    ev = []
    progression = [['A2', 'C4', 'E4', 'A4'], ['G2', 'B3', 'D4', 'G4'], ['F2', 'A3', 'C4', 'F4'], ['E2', 'G#3', 'B3', 'E4']] * 2
    for b, notes in enumerate(progression):
        ev.append((b * BAR, pluck(hz(notes[0]), 2.5, MSR, rng, 0.45) * 0.5))           # Bajo en el uno
        for k, n in enumerate(notes[1:] + notes[1:2]):                                 # Arpegio en corcheas
            ev.append((b * BAR + (k + 1) * BEAT * 0.75, pluck(hz(n), 1.6, MSR, rng) * 0.3))
    for b, n in ((3, 'F4'), (7, 'F4')):                                                # La segunda frigia
        ev.append((b * BAR + 2.5 * BEAT, pluck(hz(n), 1.4, MSR, rng) * 0.25))
    return score(8, BAR, ev)


def with_motif(bed, motif, bed_share=0.7):
    bed = bed / np.max(np.abs(bed))
    motif = motif[:len(bed)] / np.max(np.abs(motif))
    return bed * bed_share + motif * (1 - bed_share)


bed1 = loop(drone([73.42, 110.0, 146.83, 174.61], seconds) * 0.5 + low(RNG.standard_normal(int(seconds * MSR)), 400, MSR) * 0.05)
made.append(save('musica/historia1', with_motif(bed1, motif_story1()), 0.3, MSR, fade_out=False))
sea = low(RNG.standard_normal(int(seconds * MSR)), 700, MSR) * (0.6 + 0.4 * np.sin(2 * np.pi * 0.09 * t(seconds, MSR)) ** 2)
made.append(save('musica/historia2', with_motif(loop(sea * 0.7 + drone([82.41, 123.47, 164.81], seconds) * 0.35), motif_story2()), 0.3, MSR, fade_out=False))
wind = band(RNG.standard_normal(int(seconds * MSR)), 300, 1600, MSR) * (0.5 + 0.5 * np.sin(2 * np.pi * 0.05 * t(seconds, MSR)) ** 2)
made.append(save('musica/historia3', with_motif(loop(wind * 0.6 + drone([55.0, 110.0, 220.0], seconds) * 0.35), motif_story3()), 0.3, MSR, fade_out=False))
pulse = drone([41.2, 61.74], seconds) * 0.6
x = t(seconds, MSR)
pulse *= 0.55 + 0.45 * np.clip(np.sin(2 * np.pi * (70 / 60) * x), 0, 1) ** 8
made.append(save('musica/acusacion', loop(pulse), 0.35, MSR, fade_out=False))

for m in made:
    print(os.path.relpath(m))
