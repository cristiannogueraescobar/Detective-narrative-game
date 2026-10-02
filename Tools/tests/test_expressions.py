"""Tests de Tools/make_expressions.py (sesión C). Uso: python -m unittest discover Tools/tests"""
import os
import sys
import unittest

import numpy as np

sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), '..'))
import make_expressions as me  # noqa: E402
from PIL import Image  # noqa: E402


class ExpressionTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.built = {k: me.build(k) for k in me.CHARACTERS}

    def base(self, key):
        return np.asarray(Image.open(os.path.join(me.REPO, 'Assets', me.CHARACTERS[key][0])).convert('RGBA'))

    def test_las_que_faltan_segun_art_needed(self):
        self.assertEqual(12, len(self.built))
        for key, exprs in self.built.items():
            self.assertEqual(2, len(exprs), key)
            self.assertTrue(set(exprs) <= {'triste', 'nervioso', 'enfadado'}, key)

    def test_mismo_encuadre(self):
        # El mismo tamaño y la misma silueta (diferencia de opacidad ≤ 1 %): el retrato no "salta" al cambiar de estado
        for key, exprs in self.built.items():
            base = self.base(key)
            for kind, img in exprs.items():
                a = np.asarray(img)
                self.assertEqual(base.shape, a.shape, f'{key}_{kind}')
                diff = ((base[..., 3] > 128) != (a[..., 3] > 128)).mean()
                self.assertLessEqual(diff, 0.01, f'{key}_{kind}: {diff:.3%} de la silueta cambia')

    def test_solo_cambia_la_cara(self):
        # Todo lo que cambia está en el 35 % superior de la figura (la cabeza)
        for key, exprs in self.built.items():
            base = self.base(key)
            ys = np.where((base[..., 3] > 128).any(1))[0]
            limit = ys.min() + int((ys.max() - ys.min()) * 0.35)
            for kind, img in exprs.items():
                changed = np.abs(np.asarray(img).astype(int) - base.astype(int)).max(-1) > 0
                rows = np.where(changed.any(1))[0]
                self.assertTrue(len(rows) > 0, f'{key}_{kind} no cambia nada')
                self.assertLess(rows.max(), limit, f'{key}_{kind} cambia por debajo de la cabeza')


if __name__ == '__main__':
    unittest.main()
