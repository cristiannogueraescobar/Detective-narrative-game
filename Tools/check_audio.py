"""Control de calidad del audio de Assets/Resources/Audio: pico, desplazamiento DC, silencio inicial (5 ms en los
efectos, lo pide AUDIO-NEEDED.md) y salto en el punto de bucle de la música. Sale con código 1 si algo falla.
Uso: python Tools/check_audio.py
"""
import glob
import os
import sys

import numpy as np
from scipy.io import wavfile

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'Assets', 'Resources', 'Audio')
failures = []
print(f"{'archivo':32} {'s':>6} {'pico':>6} {'rms':>6} {'dc':>7} {'silencio ms':>11} {'salto bucle':>11}")
for path in sorted(glob.glob(os.path.join(ROOT, '**', '*.wav'), recursive=True)):
    sr, d = wavfile.read(path)
    d = d.astype(float) / 32768.0
    if d.ndim > 1:
        d = d.mean(axis=1)
    name = os.path.relpath(path, ROOT).replace(os.sep, '/')
    peak, rms, dc = np.abs(d).max(), np.sqrt((d ** 2).mean()), d.mean()
    lead = np.argmax(np.abs(d) > 0.002) / sr * 1000
    is_music = name.startswith('musica/')
    # Salto de bucle comparado con el paso típico entre muestras (el ruido de lluvia ya salta mucho)
    typical = np.percentile(np.abs(np.diff(d)), 99)
    jump = abs(d[0] - d[-1])
    print(f"{name:32} {len(d) / sr:6.2f} {peak:6.2f} {rms:6.3f} {dc:7.4f} {lead:11.1f} {jump / max(typical, 1e-6):10.2f}x")
    if peak > 0.95:
        failures.append(f'{name}: pico {peak:.2f} (satura)')
    if abs(dc) > 0.01:
        failures.append(f'{name}: DC {dc:.3f}')
    if not is_music and lead < 4:
        failures.append(f'{name}: {lead:.1f} ms de silencio inicial (< 5)')
    if is_music and jump > typical * 1.5:
        failures.append(f'{name}: salto en el bucle {jump:.3f} (> 1,5 × el paso típico {typical:.3f})')

print('\n'.join(['', 'FALLOS:'] + failures) if failures else '\nTodo bien.')
sys.exit(1 if failures else 0)
