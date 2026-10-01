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


def figure_with_fringe():
    """Figura sobre la rejilla de 5 px: cuerpo verde oliva sin contorno, un halo claro de 1 celda alrededor (bordes
    antialiasados del generador), una mota suelta y sombra semitransparente a los pies."""
    a = np.zeros((1024, 768, 4), np.uint8)
    a[100:985, 300:470] = (90, 100, 50, 255)        # Cuerpo (múltiplos de 5: alineado con la rejilla)
    a[95:100, 295:475] = (230, 225, 210, 255)       # Halo claro arriba
    a[100:985, 295:300] = (230, 225, 210, 255)      # Halo claro a la izquierda
    a[500:510, 100:110] = (120, 80, 40, 255)        # Mota suelta lejos de la figura
    a[985:995, 470:540] = (0, 0, 0, 47)             # Sombra
    return Image.fromarray(a, 'RGBA')


class OutlineTests(unittest.TestCase):
    def setUp(self):
        self.out = np.asarray(style.reinforce_outline(figure_with_fringe(), cell=5))

    def test_el_contorno_es_negro_y_de_dos_celdas(self):
        row = self.out[500]
        xs = np.where(row[:, 3] == 255)[0]
        xs = xs[xs > 200]
        left = xs.min()
        self.assertTrue((row[left:left + 10, :3] == 0).all(), 'dos celdas (10 px) de negro en el borde')
        self.assertFalse((row[left + 10, :3] == 0).all(), 'y no más: el interior conserva su color')

    def test_quita_el_halo_claro_y_las_motas(self):
        opaque = self.out[..., 3] == 255
        self.assertFalse(opaque[500:510, 100:110].any(), 'mota suelta')
        light = opaque & (self.out[..., :3].min(-1) > 200)
        self.assertFalse(light.any(), 'nada claro en el borde')

    def test_no_toca_el_interior_ni_la_sombra(self):
        self.assertTrue((self.out[500, 380, :3] == (90, 100, 50)).all())
        self.assertTrue((self.out[990, 500] == (0, 0, 0, 47)).all())

    def test_el_grosor_queda_en_el_rango_de_los_originales(self):
        m = style.outline_thickness(self.out) / (985 - 100)
        self.assertTrue(0.008 <= m <= 0.024, m)


if __name__ == '__main__':
    unittest.main()
