"""Posproceso de un retrato generado (raw → final), sin GPU, para iterar rápido y para que generate.py y quien
reprocese usen exactamente la misma cadena:
  remove_white_bg (fondo fuera, 768x1024) → sombra sintética si hace falta → rejilla de 5 px → contorno negro →
  color (piel de Marcos y Lucía) → contraste.

Prueba ciega 1 (3 de 3 señalaron al candidato por "paleta turbia, sin contraste"): medido, los candidatos tenían 35-38
colores frente a 2600-3200 de los originales (la rejilla cuantizaba a 40 y aplastaba las luces) y un contraste (desv.
de L*) de 19-21 frente a 22,5-27. Ahora: 160 colores y el contraste estirado hacia 24.

Uso: python Tools/portrait_gen/finish.py <raw.png | carpeta raw> <salida.png | carpeta final>"""
import os
import sys

import numpy as np
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path[:0] = [HERE, os.path.join(HERE, '..')]

import colour  # noqa: E402
import remove_white_bg as rwb  # noqa: E402
import style  # noqa: E402

COLOURS = 160
TARGET_CONTRAST = 24.0  # Desviación de L* en la figura: Marcos 27,3, Lucía 22,5, Álex 23,2
MAX_STRETCH = 1.3
TARGET_CHROMA = 37.0    # Croma media de la figura: Marcos 34,1, Lucía 51,8, Álex 26,3
MAX_SATURATE = 1.35


def stretch_contrast(rgba, target=TARGET_CONTRAST):
    """Estira L* alrededor de su media hasta la desviación objetivo (como mucho ×1,3). Ni el contorno (L* < 15) ni la
    sombra semitransparente se tocan."""
    out = rgba.copy()
    opaque = rgba[..., 3] == 255
    lab = colour.srgb_to_lab(rgba[..., :3].astype(float))
    fig = opaque & (lab[..., 0] >= 15)
    if fig.sum() < 100:
        return out
    mean, sd = lab[..., 0][fig].mean(), lab[..., 0][fig].std()
    k = min(MAX_STRETCH, max(1.0, target / max(sd, 1e-6)))
    lab[..., 0] = np.where(fig, np.clip(mean + (lab[..., 0] - mean) * k, 15, 100), lab[..., 0])
    out[..., :3] = np.where(fig[..., None], colour.lab_to_srgb(lab), rgba[..., :3]).astype(np.uint8)
    return out


def saturate(rgba, target=TARGET_CHROMA):
    """Prueba ciega 2 (3 de 3): "paleta turbia, poco saturada". Escala a* y b* de la figura hasta la croma media
    objetivo (como mucho ×1,35), sin tocar contorno ni sombra."""
    out = rgba.copy()
    opaque = rgba[..., 3] == 255
    lab = colour.srgb_to_lab(rgba[..., :3].astype(float))
    fig = opaque & (lab[..., 0] >= 15)
    if fig.sum() < 100:
        return out
    chroma = np.hypot(lab[..., 1], lab[..., 2])[fig].mean()
    k = min(MAX_SATURATE, max(1.0, target / max(chroma, 1e-6)))
    # Solo la ropa: la piel ya va a la de Marcos y Lucía (colour.correct) y saturada se volvía amarillo fosforito
    cloth = fig & ~colour.skin_mask(rgba[..., :3], opaque)
    lab[..., 1:] = np.where(cloth[..., None], lab[..., 1:] * k, lab[..., 1:])
    out[..., :3] = np.where(fig[..., None], colour.lab_to_srgb(lab), rgba[..., :3]).astype(np.uint8)
    return out


def finish(raw, cell=5, outline=True):
    """Devuelve (final RGBA, informe de remove_white_bg)."""
    clean, report = rwb.process(raw)
    if report['shadow_pixels'] == 0:
        clean = style.add_shadow(clean)
    final = style.pixelate(clean, cell=cell, colours=COLOURS)
    final = style.cel_flatten(final, cell=cell)  # Prueba ciega 2: "sombreado moteado"
    if outline:
        final = style.reinforce_outline(final, cell=cell)
    rgba = colour.correct(np.asarray(final))
    rgba = stretch_contrast(rgba)
    rgba = saturate(rgba)
    return Image.fromarray(rgba, 'RGBA'), report


def main(src, dst):
    if os.path.isdir(src):
        os.makedirs(dst, exist_ok=True)
        for name in sorted(os.listdir(src)):
            if name.endswith('.png'):
                finish(Image.open(os.path.join(src, name)))[0].save(os.path.join(dst, name))
                print(name, flush=True)
    else:
        finish(Image.open(src))[0].save(dst)
        print(dst)


if __name__ == '__main__':
    main(sys.argv[1], sys.argv[2])
