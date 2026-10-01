"""Esqueleto de pose en formato OpenPose dibujado por código (sin detector: los pesos de OpenPose no permiten uso
comercial). Las proporciones se fijan aquí: cabeza ~1/5 de la figura, figura al 90 % del alto, como Marcos y Álex
(docs/art/javier/BRIEF.md, sección 3)."""
import math

from PIL import Image, ImageDraw

# Orden COCO-18 y colores/segmentos del render estándar de OpenPose (lo que espera el ControlNet de pose)
LIMBS = [(1, 2), (1, 5), (2, 3), (3, 4), (5, 6), (6, 7), (1, 8), (8, 9), (9, 10), (1, 11), (11, 12), (12, 13),
         (1, 0), (0, 14), (14, 16), (0, 15), (15, 17)]
COLORS = [(255, 0, 0), (255, 85, 0), (255, 170, 0), (255, 255, 0), (170, 255, 0), (85, 255, 0), (0, 255, 0),
          (0, 255, 85), (0, 255, 170), (0, 255, 255), (0, 170, 255), (0, 85, 255), (0, 0, 255), (85, 0, 255),
          (170, 0, 255), (255, 0, 255), (255, 0, 170), (255, 0, 85)]


def standing_three_quarter(width=864, height=1152, head_ratio=0.19, fill=0.90, heavy=True):
    """Hombre adulto de pie, de frente girado tres cuartos, brazos caídos (la mano derecha del personaje, a la
    izquierda de la imagen, sujeta una botella). Devuelve 18 puntos (x, y) en píxeles."""
    top = height * (1 - fill) / 2
    fh = height * fill
    cx = width / 2
    head = fh * head_ratio
    y = lambda f: top + f * fh                      # Fracción de la figura (0 = coronilla, 1 = suelas)
    sw = fh * (0.145 if heavy else 0.125)            # Media anchura de hombros
    hw = fh * (0.075 if heavy else 0.065)            # Media anchura de caderas
    shoulders = y(head_ratio + 0.06)
    return [
        (cx + 6, top + head * 0.62),                 # 0 nariz
        (cx, shoulders - fh * 0.01),                 # 1 cuello (base, entre los hombros)
        (cx - sw, shoulders),                        # 2 hombro derecho del personaje
        (cx - sw * 1.12, y(0.40)),                   # 3 codo derecho
        (cx - sw * 1.05, y(0.53)),                   # 4 muñeca derecha (botella)
        (cx + sw * 0.9, shoulders),                  # 5 hombro izquierdo (algo más estrecho: tres cuartos)
        (cx + sw * 1.02, y(0.40)),                   # 6 codo izquierdo
        (cx + sw * 0.92, y(0.53)),                   # 7 muñeca izquierda
        (cx - hw, y(0.52)),                          # 8 cadera derecha
        (cx - hw * 1.05, y(0.75)),                   # 9 rodilla derecha
        (cx - hw * 1.1, y(0.97)),                    # 10 tobillo derecho
        (cx + hw * 0.95, y(0.52)),                   # 11 cadera izquierda
        (cx + hw, y(0.75)),                          # 12 rodilla izquierda
        (cx + hw * 1.05, y(0.97)),                   # 13 tobillo izquierdo
        (cx - head * 0.13, top + head * 0.45),       # 14 ojo derecho
        (cx + head * 0.19, top + head * 0.45),       # 15 ojo izquierdo
        (cx - head * 0.30, top + head * 0.50),       # 16 oreja derecha
        (cx + head * 0.30, top + head * 0.50),       # 17 oreja izquierda
    ]


def render(points, width=864, height=1152, stick=8, radius=6):
    """Render estándar: fondo negro, segmentos como elipses al 60 % y puntos encima."""
    canvas = Image.new('RGB', (width, height), (0, 0, 0))
    for i, (a, b) in enumerate(LIMBS):
        layer = Image.new('RGB', (width, height), (0, 0, 0))
        (x0, y0), (x1, y1) = points[a], points[b]
        mx, my = (x0 + x1) / 2, (y0 + y1) / 2
        length = math.hypot(x1 - x0, y1 - y0) / 2
        angle = math.atan2(y1 - y0, x1 - x0)
        poly = [(mx + length * math.cos(t) * math.cos(angle) - stick * math.sin(t) * math.sin(angle),
                 my + length * math.cos(t) * math.sin(angle) + stick * math.sin(t) * math.cos(angle))
                for t in [k * 2 * math.pi / 36 for k in range(36)]]
        ImageDraw.Draw(layer).polygon(poly, fill=COLORS[i])
        mask = Image.new('L', (width, height), 0)
        ImageDraw.Draw(mask).polygon(poly, fill=int(255 * 0.6))
        canvas = Image.composite(Image.blend(canvas, layer, 1.0), canvas, mask)
    draw = ImageDraw.Draw(canvas)
    for i, (x, y) in enumerate(points):
        draw.ellipse((x - radius, y - radius, x + radius, y + radius), fill=COLORS[i])
    return canvas


if __name__ == '__main__':
    import sys
    render(standing_three_quarter()).save(sys.argv[1] if len(sys.argv) > 1 else 'pose.png')
