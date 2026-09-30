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


seconds = 31.5
made.append(save('musica/menu', loop(rain(seconds) * 0.8 + drone([55.0, 82.41, 110.0], seconds) * 0.25), 0.35, MSR, fade_out=False))
made.append(save('musica/historia1', loop(drone([73.42, 110.0, 146.83, 174.61], seconds) * 0.5 + low(RNG.standard_normal(int(seconds * MSR)), 400, MSR) * 0.05), 0.3, MSR, fade_out=False))
sea = low(RNG.standard_normal(int(seconds * MSR)), 700, MSR) * (0.6 + 0.4 * np.sin(2 * np.pi * 0.09 * t(seconds, MSR)) ** 2)
made.append(save('musica/historia2', loop(sea * 0.7 + drone([65.41, 98.0, 130.81], seconds) * 0.35), 0.3, MSR, fade_out=False))
wind = band(RNG.standard_normal(int(seconds * MSR)), 300, 1600, MSR) * (0.5 + 0.5 * np.sin(2 * np.pi * 0.05 * t(seconds, MSR)) ** 2)
made.append(save('musica/historia3', loop(wind * 0.6 + drone([61.74, 92.5, 123.47], seconds) * 0.35), 0.3, MSR, fade_out=False))
pulse = drone([41.2, 61.74], seconds) * 0.6
x = t(seconds, MSR)
pulse *= 0.55 + 0.45 * np.clip(np.sin(2 * np.pi * (70 / 60) * x), 0, 1) ** 8
made.append(save('musica/acusacion', loop(pulse), 0.35, MSR, fade_out=False))

for m in made:
    print(os.path.relpath(m))
