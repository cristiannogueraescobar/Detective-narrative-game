"""Tests de los retratos derivados (Tools/make_derived_portraits.py). Uso: python -m unittest discover Tools/tests"""
import os
import sys
import unittest

import numpy as np

sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), '..'))
import make_derived_portraits as mdp  # noqa: E402

W, H = 760, 1158  # padre.gif.png


def region(img, x0, y0, x1, y1, mirror=True):
    """Zona en coordenadas del original; el derivado de Javier va en espejo."""
    a = np.asarray(img.convert('RGBA')).astype(float)
    if mirror:
        x0, x1 = W - x1, W - x0
    return a[y0:y1, x0:x1]


class JavierTests(unittest.TestCase):
    """Sesión C: Javier tenía la cara y el bigote de Daniel. Tiene que dejar de parecer la misma persona."""

    @classmethod
    def setUpClass(cls):
        cls.img = mdp.javier()

    def test_no_tiene_bigote(self):
        # Entre la nariz y la boca de Daniel hay un bigote casi negro; en Javier, piel
        lip = region(self.img, 300, 285, 400, 315)
        dark = (lip[..., :3].max(-1) < 70) & (lip[..., 3] > 128)
        self.assertLess(dark.mean(), 0.05, f'{dark.mean():.2f} del labio superior es negro')

    def test_tiene_barba_de_dias(self):
        cheek = region(self.img, 440, 280, 460, 300)[..., :3].mean()  # mejilla derecha, fuera de la barba
        chin = region(self.img, 320, 350, 420, 375)[..., :3].mean()
        self.assertLess(chin, cheek * 0.8, 'el mentón, más oscuro que la mejilla')

    def test_parpados_caidos(self):
        # La parte de arriba de los ojos de Daniel es blanca (ojos muy abiertos); Javier los tiene entornados
        lids = region(self.img, 290, 225, 345, 238)
        white = (lids[..., :3].min(-1) > 220) & (lids[..., 3] > 128)
        self.assertLess(white.mean(), 0.02)


if __name__ == '__main__':
    unittest.main()
