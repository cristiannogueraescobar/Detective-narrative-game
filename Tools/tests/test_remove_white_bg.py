"""Tests de Tools/remove_white_bg.py con imágenes sintéticas. Uso: python -m unittest discover Tools/tests"""
import os
import sys
import unittest

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), '..'))
import remove_white_bg as rwb  # noqa: E402

WHITE, BLACK, SHIRT, SKIN = (255, 255, 255), (0, 0, 0), (250, 250, 248), (200, 140, 90)
SHADOW = (205, 205, 208)  # Gris claro que pinta un generador como sombra suave sobre blanco


def synthetic(w=600, h=900, shadow=True):
    """Figura con contorno negro cerrado, camisa casi blanca dentro, cara y sombra gris a la derecha de los pies."""
    a = np.zeros((h, w, 3), np.uint8)
    a[:] = WHITE
    x0, x1, y0, y1 = 200, 400, 80, 800          # Figura: de y0 (cabeza) a y1 (suelas)
    a[y0:y1, x0:x1] = BLACK                       # Contorno grueso (relleno y luego vaciado)
    a[y0 + 12:y1 - 12, x0 + 12:x1 - 12] = SHIRT   # Camisa blanca: NO debe volverse transparente
    a[y0 + 20:y0 + 120, x0 + 60:x1 - 60] = SKIN   # Cara
    if shadow:
        a[y1 - 10:y1, x1:x1 + 70] = SHADOW        # Sombra a la derecha de los pies, a la altura de la suela
    a[300:330, 20:50] = (254, 254, 254)           # Casi blanco suelto en el fondo: también es fondo
    return Image.fromarray(a, 'RGB')


class RemoveWhiteBackgroundTests(unittest.TestCase):
    def setUp(self):
        self.out, self.report = rwb.process(synthetic())
        self.a = np.asarray(self.out)

    def test_sale_a_768x1024_rgba(self):
        self.assertEqual(self.out.size, (768, 1024))
        self.assertEqual(self.out.mode, 'RGBA')

    def test_el_fondo_del_borde_es_transparente(self):
        for y, x in [(0, 0), (0, 767), (1023, 0), (1023, 767), (500, 10)]:
            self.assertEqual(self.a[y, x, 3], 0, (y, x))

    def test_la_camisa_blanca_dentro_del_contorno_sigue_opaca(self):
        ys, xs = np.where(self.a[..., 3] == 255)
        cy, cx = int(ys.mean()), int(xs.mean())
        self.assertEqual(self.a[cy, cx, 3], 255)
        self.assertGreater(self.a[cy, cx, :3].min(), 240, 'el centro de la figura es la camisa blanca')

    def test_conserva_la_sombra_semitransparente_junto_a_los_pies(self):
        semi = (self.a[..., 3] > 10) & (self.a[..., 3] < 200)
        self.assertGreater(semi.sum(), 50, 'la sombra se pierde')
        ys, xs = np.where(semi)
        opaque_y = np.where(self.a[..., 3] == 255)[0]
        self.assertGreater(ys.min(), opaque_y.max() - 60, 'la sombra está a la altura de los pies, no arriba')
        self.assertTrue(rwb.process(synthetic(shadow=False))[1]['shadow_pixels'] == 0)

    def test_los_pies_quedan_a_unos_40px_del_borde_inferior_y_la_figura_al_90_por_ciento(self):
        ys = np.where(self.a[..., 3] == 255)[0]
        self.assertAlmostEqual(1024 - (ys.max() + 1), rwb.FEET_MARGIN, delta=2)
        self.assertAlmostEqual((ys.max() + 1 - ys.min()) / 1024, rwb.FIGURE_FILL, delta=0.02)

    def test_escala_por_vecino_mas_cercano_sin_colores_nuevos(self):
        opaque = self.a[self.a[..., 3] == 255][:, :3]
        colours = {tuple(c) for c in opaque}
        self.assertTrue(colours <= {BLACK, SHIRT, SKIN}, colours - {BLACK, SHIRT, SKIN})

    def test_avisa_de_bordes_claros_o_semitransparentes_alrededor_de_la_figura(self):
        img = np.asarray(synthetic()).copy()
        ring = np.zeros(img.shape[:2], bool)
        ring[76:804, 196:404] = True
        ring[80:800, 200:400] = False
        img[ring] = (225, 225, 225)  # Halo claro de 4 px alrededor de toda la figura (bordes antialiasados de más)
        _, report = rwb.process(Image.fromarray(img))
        self.assertTrue(any('halo' in w for w in report['warnings']), report['warnings'])
        self.assertFalse(any('halo' in w for w in self.report['warnings']), self.report['warnings'])

    def test_el_fondo_blanco_encerrado_entre_brazo_y_cuerpo_tambien_es_transparente(self):
        # Caso real (Álex): el hueco entre el cable de los auriculares y la mano quedaba blanco y opaco
        img = np.asarray(synthetic()).copy()
        img[400:470, 250:320] = BLACK               # Brazo con contorno...
        img[408:462, 258:312] = WHITE               # ...que encierra un hueco de fondo blanco puro
        out, _ = rwb.process(Image.fromarray(img))
        a = np.asarray(out)
        hole = np.where((np.asarray(out)[..., :3] == 255).all(-1) & (a[..., 3] == 255))
        self.assertEqual(len(hole[0]), 0, 'queda fondo blanco puro opaco dentro de la figura')
        self.assertGreater((a[..., 3] == 255).sum(), 0)

    def test_no_avisa_de_halo_con_el_antialiasing_normal_de_los_retratos_actuales(self):
        # Marcos (02) sobre blanco: su borde suavizado es el estilo de referencia, no un fallo
        path = os.path.join(os.path.dirname(__file__), '..', '..', 'Assets', 'Images', 'Suspects', 'duenio_bar.gif.png')
        orig = Image.open(path).convert('RGBA')
        white = Image.new('RGBA', orig.size, (255, 255, 255, 255))
        white.alpha_composite(orig)
        _, report = rwb.process(white.convert('RGB'))
        self.assertFalse(any('halo' in w for w in report['warnings']), report['warnings'])

    def test_quita_tambien_un_fondo_liso_de_otro_color(self):
        # Caso real: con el LoRA de pixel art, SDXL pinta fondo gris oscuro aunque se le pida blanco (8 de 8)
        img = np.asarray(synthetic(shadow=False)).copy()
        bg = (img == 255).all(-1)
        img[bg] = (46, 44, 48)                       # Fondo gris carbón, liso
        img[300:330, 20:50] = (50, 47, 52)           # Variación suave del fondo (viñeta): también es fondo
        out, report = rwb.process(Image.fromarray(img))
        a = np.asarray(out)
        for y, x in [(0, 0), (1023, 767), (500, 10)]:
            self.assertEqual(a[y, x, 3], 0, (y, x))
        opaque = a[a[..., 3] == 255][:, :3]
        self.assertTrue({tuple(c) for c in opaque} <= {BLACK, SHIRT, SKIN}, 'el contorno negro no es fondo')
        self.assertIn('fondo de color', ' '.join(report['notes']))

    def test_quita_el_suelo_encerrado_entre_los_pies(self):
        # Caso real (Javier 5004, 5013): el generador pinta un parche de suelo algo distinto del fondo entre los pies,
        # que quedaba pegado a la figura como un "puente" beige
        img = np.full((900, 600, 3), (178, 178, 182), np.uint8)       # Fondo gris claro liso
        img[80:760, 230:370] = BLACK                                   # Cuerpo con contorno
        img[92:748, 242:358] = SHIRT
        img[700:800, 200:270] = (90, 55, 30)                           # Pie derecho (bota marrón)
        img[700:800, 330:400] = (90, 55, 30)                           # Pie izquierdo
        img[770:800, 270:330] = (150, 140, 118)                        # Parche de suelo beige entre los pies
        img[600:690, 250:350] = (150, 150, 140)                        # Pantalón gris parecido: NO es suelo
        out, _ = rwb.process(Image.fromarray(img))
        a = np.asarray(out)
        ys, xs = np.where(a[..., 3] == 255)
        sole = ys.max()
        left_foot_right_edge = np.where(a[sole - 5, :, 3] == 255)[0]
        gaps = np.diff(left_foot_right_edge)
        self.assertTrue((gaps > 5).any(), 'entre los pies tiene que quedar un hueco transparente')
        trousers = a[int(ys.min() + (sole - ys.min()) * 0.72), :, 3]
        self.assertGreater((trousers == 255).sum(), 50, 'el pantalón gris de más arriba sigue opaco')

    def test_avisa_si_el_generador_pinto_el_patron_de_cuadros(self):
        img = np.asarray(synthetic()).copy()
        yy, xx = np.mgrid[0:img.shape[0], 0:img.shape[1]]
        checker = ((yy // 16 + xx // 16) % 2 == 0)
        bg = (img == 255).all(-1)
        img[bg & checker] = (204, 204, 204)
        _, report = rwb.process(Image.fromarray(img))
        self.assertTrue(any('cuadros' in w for w in report['warnings']), report['warnings'])


if __name__ == '__main__':
    unittest.main()
