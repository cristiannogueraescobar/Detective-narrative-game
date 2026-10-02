"""Retratos derivados (Sesión A, bloque 3): los personajes que compartían retrato tienen el suyo, hecho por código a
partir de los originales (recolor por zonas y tonos, espejo, pelo con canas, accesorios), en archivos NUEVOS:
Assets/Art/Derived/<clave>.png. El arte original (Assets/Images/Suspects) no se toca.

Uso: python Tools/make_derived_portraits.py [--preview carpeta]
"""
import os
import sys

import numpy as np
from PIL import Image

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..')
SRC = os.path.join(ROOT, 'Assets', 'Images', 'Suspects')
OUT = os.path.join(ROOT, 'Assets', 'Art', 'Derived')


# ---------- color ----------
def rgb_to_hsv(rgb):
    r, g, b = rgb[..., 0], rgb[..., 1], rgb[..., 2]
    mx = rgb.max(-1)
    mn = rgb.min(-1)
    d = mx - mn
    h = np.zeros_like(mx)
    m = d > 1e-6
    rc = np.where(m, (mx - r) / np.where(m, d, 1), 0)
    gc = np.where(m, (mx - g) / np.where(m, d, 1), 0)
    bc = np.where(m, (mx - b) / np.where(m, d, 1), 0)
    h = np.where(r == mx, bc - gc, np.where(g == mx, 2.0 + rc - bc, 4.0 + gc - rc))
    h = np.where(m, (h / 6.0) % 1.0, 0)
    s = np.where(mx > 1e-6, d / np.where(mx > 1e-6, mx, 1), 0)
    return np.stack([h, s, mx], -1)


def hsv_to_rgb(hsv):
    h, s, v = hsv[..., 0], hsv[..., 1], hsv[..., 2]
    i = np.floor(h * 6.0).astype(int) % 6
    f = h * 6.0 - np.floor(h * 6.0)
    p = v * (1 - s)
    q = v * (1 - s * f)
    t = v * (1 - s * (1 - f))
    conds = [i == k for k in range(6)]
    r = np.select(conds, [v, q, p, p, t, v])
    g = np.select(conds, [t, v, v, q, p, p])
    b = np.select(conds, [p, p, t, v, v, q])
    return np.stack([r, g, b], -1)


class Portrait:
    def __init__(self, base):
        img = Image.open(os.path.join(SRC, base + '.gif.png')).convert('RGBA')
        self.a = np.array(img).astype(float) / 255.0
        self.h, self.w = self.a.shape[:2]
        self.hsv = rgb_to_hsv(self.a[..., :3])
        ys, xs = np.mgrid[0:self.h, 0:self.w]
        self.y = ys / self.h
        self.x = xs / self.w
        self.opaque = self.a[..., 3] > 0.5

    # Zona: (x0, y0, x1, y1) en coordenadas relativas del original
    def zone(self, *rects):
        m = np.zeros((self.h, self.w), bool)
        for x0, y0, x1, y1 in rects:
            m |= (self.x >= x0) & (self.x < x1) & (self.y >= y0) & (self.y < y1)
        return m

    def color(self, h=None, s=None, v=None):
        """Máscara por rangos de tono (0-1, admite vuelta: (0.95, 0.05)), saturación y valor."""
        H, S, V = self.hsv[..., 0], self.hsv[..., 1], self.hsv[..., 2]
        m = self.opaque.copy()
        if h is not None:
            lo, hi = h
            m &= ((H >= lo) & (H <= hi)) if lo <= hi else ((H >= lo) | (H <= hi))
        if s is not None:
            m &= (S >= s[0]) & (S <= s[1])
        if v is not None:
            m &= (V >= v[0]) & (V <= v[1])
        return m

    def recolor(self, mask, hue=None, hue_shift=0.0, sat=1.0, sat_set=None, val=1.0, val_add=0.0):
        hsv = self.hsv
        H = hsv[..., 0].copy()
        if hue is not None:
            H = np.where(mask, hue, H)
        H = np.where(mask, (H + hue_shift) % 1.0, H)
        S = np.where(mask, np.clip(hsv[..., 1] * sat if sat_set is None else sat_set, 0, 1), hsv[..., 1])
        V = np.where(mask, np.clip(hsv[..., 2] * val + val_add, 0, 1), hsv[..., 2])
        self.hsv = np.stack([H, S, V], -1)

    def erase(self, mask):
        self.a[..., 3] = np.where(mask, 0, self.a[..., 3])
        self.opaque &= ~mask

    def paint(self, mask, rgb):
        """Pinta píxeles opacos con un color fijo (accesorios)."""
        r, g, b = [c / 255.0 for c in rgb]
        self.hsv = np.where(mask[..., None], rgb_to_hsv(np.array([[[r, g, b]]]))[0, 0], self.hsv)
        self.a[..., 3] = np.where(mask, 1.0, self.a[..., 3])
        self.opaque |= mask

    def image(self, mirror=False):
        rgb = hsv_to_rgb(self.hsv)
        out = np.concatenate([rgb, self.a[..., 3:4]], -1)
        img = Image.fromarray((np.clip(out, 0, 1) * 255).astype(np.uint8), 'RGBA')
        return img.transpose(Image.FLIP_LEFT_RIGHT) if mirror else img


# ---------- recetas ----------
WARM = (0.02, 0.16)  # Piel y amarillos (separados por zona)


def px(p, x0, y0, x1, y1):
    """Zona en píxeles del original (no relativa)."""
    return p.zone((x0 / p.w, y0 / p.h, x1 / p.w, y1 / p.h))


def javier():
    """Javier Romero, 44, olivarero: camisa de trabajo de cuadros oliva y caqui, pantalón de pana marrón, piel curtida.
    Sobre el padre, en espejo (Daniel conserva el original).
    Sesión C: tenía la cara y el bigote de Daniel. Ahora, editando píxeles de la cara sobre la rejilla del original:
    sin bigote, barba de días que sigue la mandíbula, cejas bajas y pobladas, párpados caídos, ojeras rojizas, pelo casi
    negro con las sienes canosas y franjas verticales que convierten las rayas en cuadros."""
    p = Portrait('padre')
    torso = p.zone((0.30, 0.335, 0.90, 0.565)) & ~p.zone((0.555, 0.49, 0.95, 0.65)) & ~p.zone((0.53, 0.44, 0.735, 0.56))
    p.recolor(torso & p.color(s=(0, 0.22), v=(0.75, 1)), hue=0.13, sat_set=0.30, val=0.86)        # rayas blancas → caqui claro
    p.recolor(torso & p.color(h=WARM, s=(0.45, 1), v=(0.55, 1)), hue=0.17, sat=0.75, val=0.62)    # rayas amarillas → oliva
    p.recolor(p.zone((0.30, 0.30, 0.90, 0.56)) & p.color(h=(0.30, 0.50), s=(0.4, 1)), hue=0.08, sat=0.6, val=0.7)  # cuello verde → marrón
    p.recolor(p.zone((0, 0.55, 1, 0.92)) & p.color(h=(0.50, 0.70), s=(0.15, 1)), hue=0.07, sat=0.65, val=0.75)  # vaqueros → pana marrón
    skin = p.color(h=WARM, s=(0.35, 1), v=(0.5, 1)) & ~torso & ~p.zone((0, 0.88, 1, 1))
    p.recolor(skin, hue_shift=-0.012, sat=1.05, val=0.90)                                          # más curtido

    face = px(p, 270, 140, 470, 380)
    # Sin bigote: todo el labio superior (el bigote tenía un borde claro), con el color de su mejilla
    cheek = px(p, 300, 230, 330, 260) & p.color(h=WARM, s=(0.3, 1), v=(0.5, 1))
    cheek_rgb = np.median(hsv_to_rgb(p.hsv)[cheek], axis=0)
    p.paint(px(p, 280, 278, 420, 330) & p.opaque, tuple(int(c * 255) for c in cheek_rgb))
    # Barba de días: tono plano (como el sombreado de los originales) que sigue la mandíbula, más oscura abajo
    jaw = (px(p, 300, 318, 440, 385) | px(p, 265, 335, 475, 385)) & p.color(h=WARM, s=(0.3, 1), v=(0.45, 1))
    p.recolor(jaw, hue_shift=-0.02, sat=0.80, val=0.62)
    p.recolor(jaw & ~px(p, 265, 300, 475, 368), val=0.85)
    p.paint(px(p, 310, 333, 400, 343), (70, 30, 20))                                             # boca recta, cansada
    for x0, x1 in ((285, 330), (395, 450)):                                                       # cejas bajas y pobladas
        p.paint(px(p, x0, 205, x1, 222) & face, (40, 22, 14))
    for x0, x1 in ((280, 330), (385, 430)):                                                       # ojeras rojizas
        p.recolor(px(p, x0, 262, x1, 272) & p.color(h=WARM, v=(0.4, 1)), hue=0.02, sat=0.7, val=0.75)
    temples = (px(p, 250, 150, 290, 240) | px(p, 455, 150, 500, 240)) & p.color(h=(0.0, 0.12), s=(0.25, 1), v=(0.15, 0.75))
    p.recolor(temples, sat_set=0.10, val=1.30)                                                    # sienes canosas
    hair = px(p, 230, 60, 520, 200) & p.color(h=(0.0, 0.12), s=(0.25, 1), v=(0.12, 0.85)) & ~px(p, 285, 140, 455, 200)
    p.recolor(hair & ~temples, sat=0.45, val=0.55)                                                # pelo casi negro
    for x0, x1 in ((285, 345), (370, 432)):                                                       # párpados caídos
        p.paint(px(p, x0, 222, x1, 240) & face, (150, 82, 45))
    yy, xx = np.mgrid[0:p.h, 0:p.w]
    p.recolor(torso & ((xx // 28) % 3 == 0) & p.color(v=(0.25, 1)), val=0.72, sat=1.1)          # rayas → cuadros
    return p.image(mirror=True)


def lucia():
    """Lucía Navarro, 41, profesora en tratamiento por depresión: pelo negro, blusa azul apagado, delantal gris como
    rebeca, falda azul marino lisa, piel más pálida, gafas. Sobre la madre, en espejo."""
    p = Portrait('madre')
    hair = p.zone((0.20, 0.0, 0.82, 0.42)) & p.color(h=(0.0, 0.10), s=(0.45, 1), v=(0.05, 0.62)) & ~p.zone((0.33, 0.115, 0.66, 0.30))
    p.recolor(hair, sat=0.25, val=0.38)                                                            # pelo casi negro
    p.recolor(p.zone((0.30, 0.0, 0.70, 0.09)) & p.color(h=(0.95, 0.04), s=(0.5, 1)), hue=0.62, sat=0.6, val=0.7)  # diadema → azul
    blouse = p.zone((0.24, 0.28, 0.66, 0.47)) & p.color(h=(0.97, 0.06), s=(0.35, 1), v=(0.5, 1))
    p.recolor(blouse, hue=0.58, sat=0.35, val=0.80)                                                # blusa azul apagado
    p.recolor(p.zone((0.60, 0.28, 0.85, 0.40)) & p.color(s=(0, 0.25), v=(0.55, 1)), hue=0.60, sat_set=0.10, val=0.85)  # mangas
    apron = p.zone((0.20, 0.44, 0.72, 0.66)) & p.color(s=(0, 0.30), v=(0.45, 1))
    p.recolor(apron, hue=0.62, sat_set=0.08, val=0.62)                                             # delantal → gris rebeca
    skirt = p.zone((0.16, 0.585, 0.84, 0.775)) & ~p.zone((0.10, 0.50, 0.235, 0.62)) & p.color(h=(0.92, 0.18), s=(0.30, 1), v=(0.30, 1))
    p.recolor(skirt, hue=0.63, sat_set=0.55, val=0.42)                                             # falda azul marino lisa
    skin = p.color(h=WARM, s=(0.30, 1), v=(0.55, 1)) & ~skirt & ~blouse
    p.recolor(skin, sat=0.72, val=1.0)                                                             # más pálida
    # Gafas: dos monturas finas sobre los ojos (cara del recorte: x 0.33-0.66, ojos hacia y 0.17-0.20)
    frame = np.zeros((p.h, p.w), bool)
    for cx in (0.415, 0.575):
        rx, ry = 0.055, 0.016
        e = (((p.x - cx) / rx) ** 2 + ((p.y - 0.186) / ry) ** 2)
        frame |= (e <= 1.0) & (e >= 0.62)
    frame |= p.zone((0.47, 0.183, 0.52, 0.188))
    p.paint(frame & p.opaque, (40, 30, 30))
    return p.image(mirror=True)


def alex():
    """Álex Romero, 17: pelo negro, gorra roja y negra, chaqueta burdeos, vaqueros negros, detalles rojos en las
    zapatillas. Sobre el hermano, en espejo."""
    p = Portrait('hermano')
    p.recolor(p.zone((0.30, 0.0, 0.85, 0.19)) & p.color(h=(0.28, 0.50), s=(0.35, 1)), hue=0.99, sat=0.95, val=0.85)  # gorra → roja
    hair = p.zone((0.28, 0.02, 0.72, 0.22)) & p.color(h=(0.0, 0.10), s=(0.4, 1), v=(0.05, 0.6))
    p.recolor(hair, sat=0.2, val=0.35)                                                             # pelo negro
    jacket = p.zone((0.22, 0.20, 0.90, 0.53)) & p.color(v=(0.0, 0.40)) & ~p.color(h=WARM, s=(0.5, 1), v=(0.3, 1))
    p.recolor(jacket, hue=0.98, sat_set=0.65, val=1.0, val_add=0.12)                               # chaqueta → burdeos
    p.recolor(p.zone((0.22, 0.20, 0.90, 0.53)) & p.color(h=(0.28, 0.50), s=(0.35, 1)), hue=0.12, sat=0.4, val=1.1)  # franjas → crema
    jeans = p.zone((0.20, 0.47, 0.90, 0.88)) & p.color(h=(0.50, 0.70), s=(0.12, 1))
    p.recolor(jeans, sat=0.15, val=0.42)                                                           # vaqueros negros
    p.recolor(p.zone((0.2, 0.84, 0.95, 1)) & p.color(h=(0.28, 0.50), s=(0.35, 1)), hue=0.99, sat=0.9)  # zapatillas: rojo
    return p.image(mirror=True)


def _skin_of(p, x0, y0, x1, y1):
    """Color medio de la piel en una zona (para tapar sin que se note el parche)."""
    m = px(p, x0, y0, x1, y1) & p.color(h=WARM, s=(0.3, 1), v=(0.5, 1))
    return tuple(int(c * 255) for c in np.median(hsv_to_rgb(p.hsv)[m], axis=0))


def _mouth(p, kind, skin):
    """La "o" de sorpresa del original fuera; una boca cerrada propia de cada una (sobre la rejilla de ~8 px)."""
    p.paint(px(p, 380, 355, 440, 412) & p.opaque, skin)
    line = (110, 45, 30)
    if kind == 'amparo':      # sonrisa afable
        p.paint(px(p, 396, 384, 428, 391), line)
        p.paint(px(p, 389, 377, 397, 385) | px(p, 427, 377, 435, 385), line)
    elif kind == 'maruxa':    # boca apretada, comisuras hacia abajo (desconfiada)
        p.paint(px(p, 396, 386, 426, 393), line)
        p.paint(px(p, 389, 392, 397, 400) | px(p, 425, 392, 433, 400), line)
    else:                     # Encarna: sonrisa suave y cálida (nunca inquietante: es culpable en una variante)
        p.paint(px(p, 398, 386, 424, 392), line)
        p.paint(px(p, 392, 381, 399, 387) | px(p, 423, 381, 430, 387), line)


def _wrinkles(p, skin, depth=0.72):
    dark = tuple(int(c * depth) for c in skin)
    p.paint(px(p, 336, 318, 346, 324) | px(p, 498, 318, 508, 324), dark)          # patas de gallo
    p.paint(px(p, 374, 362, 380, 384) | px(p, 444, 362, 450, 384), dark)          # surcos junto a la boca


def vecina(kind):
    """Las tres vecinas, de 63 a 74 años (en el original parece mucho más joven): pelo con canas y ropa propia.
    Amparo (70, mira por la ventana) conserva el vestido; Maruxa (74, Galicia) azul marino y blanco, rebeca marrón;
    Encarna (63, perdió a su hija) de luto, de negro. Solo Amparo conserva el cigarro.
    Sesión C: tenían la misma cara y la misma boca de sorpresa. Ahora, editando la cara: Amparo con gafas de leer,
    arrugas y sonrisa afable; Maruxa con pañuelo oscuro, ojos entornados y boca apretada; Encarna con sonrisa suave y
    la medalla de la Virgen. Sin el cigarro en Maruxa y Encarna."""
    p = Portrait('vecina')
    hair = p.zone((0.30, 0.03, 0.66, 0.30)) & p.color(h=(0.93, 0.11), s=(0.4, 1), v=(0.05, 0.85)) & ~p.zone((0.33, 0.165, 0.55, 0.30))
    dress = p.zone((0.30, 0.27, 0.80, 0.71)) & p.color(h=(0.20, 0.48), s=(0.3, 1))
    flowers = p.zone((0.30, 0.27, 0.80, 0.71)) & p.color(h=(0.15, 0.20), s=(0.5, 1), v=(0.6, 1))
    cardigan = p.color(h=(0.72, 0.92), s=(0.3, 1))
    smoke = p.zone((0.0, 0.0, 0.19, 0.215))
    cigarette = px(p, 190, 330, 262, 395)
    skin = _skin_of(p, 340, 340, 372, 362)
    _mouth(p, kind, skin)
    if kind == 'amparo':
        p.recolor(hair, sat=0.10, val=1.55, val_add=0.05)                                         # gris plata
        _wrinkles(p, skin)
        frame = (70, 45, 30)                                                                       # gafas de leer
        for x0, y0, x1, y1 in ((344, 290, 394, 338), (424, 290, 496, 338)):
            ring = px(p, x0, y0, x1, y1) & ~px(p, x0 + 7, y0 + 7, x1 - 7, y1 - 7)
            p.paint(ring, frame)
        p.paint(px(p, 394, 302, 424, 310), frame)                                                  # puente
    elif kind == 'maruxa':
        p.recolor(hair, sat=0.05, val=2.2, val_add=0.12)                                          # blanco
        scarf = hair & ~px(p, 330, 196, 490, 232)                                                  # pañuelo; asoma el flequillo
        p.recolor(scarf, hue=0.62, sat_set=0.45, val=0.45)
        p.recolor(dress, hue=0.62, sat=0.85, val=0.55)                                             # azul marino
        p.recolor(flowers, sat_set=0.05, val=1.05)                                                 # flores blancas
        p.recolor(cardigan, hue=0.07, sat=0.6, val=0.85)                                           # rebeca marrón
        _wrinkles(p, skin, depth=0.65)
        shade = tuple(int(c * 0.75) for c in skin)                                                 # ojos entornados
        p.paint(px(p, 346, 290, 392, 306) | px(p, 426, 290, 494, 306), shade)
        p.erase(smoke | cigarette)
    elif kind == 'encarna':
        p.recolor(hair, sat=0.12, val=0.9)                                                         # gris oscuro
        p.recolor(dress | flowers, hue=0.0, sat_set=0.0, val=0.28)                                 # vestido negro (luto)
        p.recolor(flowers, val=1.6)                                                                # dibujo gris apagado
        p.recolor(cardigan, sat_set=0.05, val=0.55)                                                # rebeca gris marengo
        p.paint(px(p, 436, 468, 462, 494), (40, 28, 10))                                           # medalla de la Virgen
        p.paint(px(p, 441, 473, 457, 489), (214, 172, 64))
        p.erase(smoke | cigarette)
    return p.image(mirror=(kind == 'encarna'))


RECIPES = {
    'javier': javier,
    'lucia': lucia,
    'alex': alex,
    'amparo': lambda: vecina('amparo'),
    'maruxa': lambda: vecina('maruxa'),
    'encarna': lambda: vecina('encarna'),
}


def main():
    out = OUT
    if '--preview' in sys.argv:
        out = sys.argv[sys.argv.index('--preview') + 1]
    os.makedirs(out, exist_ok=True)
    for key, make in RECIPES.items():
        make().save(os.path.join(out, key + '.png'))
        print('ok', key)


if __name__ == '__main__':
    main()
