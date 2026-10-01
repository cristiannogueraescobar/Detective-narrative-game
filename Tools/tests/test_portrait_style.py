"""Tests de Tools/portrait_gen/style.py. Uso: python -m unittest discover Tools/tests"""
import os
import sys
import unittest

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'portrait_gen'))
import style  # noqa: E402


def two_feet():
    """Figura opaca de 768x1024 con dos pies separados que acaban en la fila 984 (pies a 40 px)."""
    a = np.zeros((1024, 768, 4), np.uint8)
    a[100:940, 300:470] = (60, 90, 40, 255)        # Cuerpo
    a[940:984, 290:370] = (80, 50, 30, 255)        # Pie derecho del personaje (izquierda de la imagen)
    a[940:984, 400:480] = (80, 50, 30, 255)        # Pie izquierdo
    return Image.fromarray(a, 'RGBA')


class SyntheticShadowTests(unittest.TestCase):
    def setUp(self):
        self.before = np.asarray(two_feet())
        self.after = np.asarray(style.add_shadow(two_feet()))

    def test_la_sombra_es_negra_y_semitransparente_como_en_los_originales(self):
        semi = (self.after[..., 3] > 0) & (self.after[..., 3] < 255)
        self.assertGreater(semi.sum(), 200)
        self.assertTrue((self.after[semi][:, :3] == 0).all(), 'sombra negra')
        self.assertTrue(np.all(np.abs(self.after[semi][:, 3].astype(int) - 47) <= 6), 'opacidad ≈18 % (46-50/255)')

    def test_cae_a_la_derecha_de_cada_pie_y_a_la_altura_de_la_suela(self):
        semi = (self.after[..., 3] > 0) & (self.after[..., 3] < 255)
        ys, xs = np.where(semi)
        self.assertGreaterEqual(ys.min(), 984 - 20, 'a la altura de la suela, no más arriba')
        self.assertLessEqual(ys.max(), 984 + 12)
        self.assertTrue(((xs > 370) & (xs < 400)).any(), 'a la derecha del pie derecho del personaje')
        self.assertTrue((xs > 480).any(), 'a la derecha del pie izquierdo')
        self.assertFalse((xs < 290).any(), 'nada a la izquierda de los pies (la luz viene de la izquierda)')

    def test_no_toca_la_figura(self):
        opaque = self.before[..., 3] == 255
        self.assertTrue(np.array_equal(self.before[opaque], self.after[opaque]))

    def test_no_duplica_una_sombra_que_ya_existe(self):
        once = style.add_shadow(two_feet())
        twice = np.asarray(style.add_shadow(once))
        self.assertTrue(np.array_equal(np.asarray(once), twice))


if __name__ == '__main__':
    unittest.main()
