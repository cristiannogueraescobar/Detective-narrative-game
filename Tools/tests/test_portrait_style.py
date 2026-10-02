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


def on_gradient(eaten_sleeve=False):
    """Figura con contorno negro sobre un fondo en degradado suave (como el de imagen a imagen: de gris a azulado)."""
    h, w = 1152, 864
    x = np.linspace(0, 1, w)[None, :, None]
    a = (np.array([172, 181, 188]) * (1 - x) + np.array([186, 205, 214]) * x).repeat(h, 0).astype(np.uint8)
    a[100:1050, 330:530] = 0                       # Contorno
    a[110:1040, 340:520] = (100, 95, 55)           # Ropa oliva
    if eaten_sleeve:
        # Manga clara sin contorno: dentro de la tolerancia del fondo (se la comería) pero con un borde (salto > 4)
        a[300:600, 250:340] = (166, 175, 181)
    return Image.fromarray(a)


class LeakTests(unittest.TestCase):
    def test_un_degradado_suave_no_cuenta_como_fuga(self):
        # Caso real (Javier por imagen a imagen): fuga 0,07-0,16 en las 8 primeras, todas con el fondo en degradado
        self.assertLess(style.background_leak(on_gradient()), 0.02)

    def test_ropa_clara_sin_contorno_si_es_fuga(self):
        self.assertGreater(style.background_leak(on_gradient(eaten_sleeve=True)), 0.05)


class CelFlattenTests(unittest.TestCase):
    """Prueba ciega 2 (3 de 3): "sombreado moteado, rayado en la camisa y el pantalón". Las motas sueltas de una celda
    se igualan a su entorno; los bordes entre zonas de color no se mueven."""

    def setUp(self):
        a = np.zeros((1024, 768, 4), np.uint8)
        a[100:900, 200:400] = (90, 100, 50, 255)       # Camisa oliva
        a[100:900, 400:600] = (80, 55, 35, 255)        # Pantalón marrón (borde vertical en x=400)
        for y in range(120, 880, 40):                  # Motas sueltas de una celda dentro de la camisa
            a[y:y + 5, 260:265] = (130, 125, 70, 255)
        self.before = a
        self.after = np.asarray(style.cel_flatten(Image.fromarray(a, 'RGBA'), cell=5))

    def test_quita_las_motas_sueltas(self):
        self.assertTrue((self.after[120:880, 260:265, :3] == (90, 100, 50)).all())

    def test_conserva_el_borde_entre_zonas(self):
        self.assertTrue((self.after[500, 395, :3] == (90, 100, 50)).all())
        self.assertTrue((self.after[500, 400, :3] == (80, 55, 35)).all())

    def test_no_toca_el_fondo_ni_el_alfa(self):
        self.assertTrue(np.array_equal(self.after[..., 3], self.before[..., 3]))


class SkinTests(unittest.TestCase):
    def test_la_ropa_ocre_no_cuenta_como_piel(self):
        # Caso real (Javier 6111): la camisa oliva-ocre cae en el mismo matiz que la piel (8-40°) y la corrección de
        # color se calculaba sobre la ropa (71 350 "píxeles de piel"). La piel se mide en la cabeza.
        sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'portrait_gen'))
        import colour
        a = np.zeros((1024, 768, 4), np.uint8)
        a[100:300, 330:430] = (225, 160, 110, 255)   # Cara (piel) en la cabeza
        a[300:960, 250:520] = (170, 130, 60, 255)    # Camisa ocre: mismo matiz que la piel
        skin = colour.skin_mask(a[..., :3], a[..., 3] == 255)
        self.assertTrue(skin[150, 380], 'la cara es piel')
        self.assertFalse(skin[600, 380], 'la camisa ocre no')

    def test_la_piel_amarilla_del_lora_cuenta_como_piel_y_se_corrige(self):
        # Prueba ciega 5 (3 de 3, primer motivo en las tres): "piel amarillo limón". El LoRA de estilo pinta la cara
        # a (243, 235, 141), matiz 56°, fuera del 8-40° de la máscara: ni se corregía ni se libraba de saturar.
        sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'portrait_gen'))
        import colour
        a = np.zeros((1024, 768, 4), np.uint8)
        a[100:300, 330:430] = (243, 235, 141, 255)   # Cara amarilla en la cabeza
        a[300:960, 250:520] = (60, 110, 50, 255)     # Camisa verde
        self.assertTrue(colour.skin_mask(a[..., :3], a[..., 3] == 255)[150, 380], 'la cara amarilla es piel')
        target = {'L': 70.0, 'a': 15.0, 'b': 25.0, 'sa': 5.0, 'sb': 5.0}
        out = colour.correct(a, target=target)
        r, g, b = (int(v) for v in out[150, 380, :3])
        self.assertGreater(r - g, 20, f'tira a melocotón, no a limón: {(r, g, b)}')

    def test_la_piel_muy_saturada_del_lora_v2_cuenta_como_piel(self):
        # Revisión de herramientas (20:52): en los brutos del LoRA v2 la piel tiene saturación > 0,8 y la máscara solo
        # veía el 0,3-0,6 % de la cabeza; sin máscara no se corregía y saturate la encendía ("piel naranja", rondas 9-12)
        sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'portrait_gen'))
        import colour
        a = np.zeros((1024, 768, 4), np.uint8)
        a[100:300, 330:430] = (255, 150, 40, 255)    # Piel saturada (s = 0,84)
        a[300:960, 250:520] = (60, 110, 50, 255)
        self.assertTrue(colour.skin_mask(a[..., :3], a[..., 3] == 255)[150, 380])

    def test_con_unos_pocos_pixeles_de_piel_no_se_corrige_la_figura(self):
        # Revisión independiente: 3 píxeles amarillos en la cabeza bastaban para mover el color de toda la figura
        sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'portrait_gen'))
        import colour
        a = np.zeros((1024, 768, 4), np.uint8)
        a[100:960, 250:520] = (60, 110, 50, 255)
        a[110, 300:303] = (243, 235, 141, 255)       # 3 píxeles "de piel"
        target = {'L': 70.0, 'a': 15.0, 'b': 25.0, 'sa': 5.0, 'sb': 5.0}
        self.assertTrue(np.array_equal(colour.correct(a, target=target), a))

    def test_sin_piel_visible_la_figura_no_se_vuelve_negra(self):
        # Caso real (sesión B, LoRA de estilo, 17:46): 13 de 26 candidatos salían como siluetas negras. Sin píxeles de
        # piel en la banda de la cabeza, la media de la piel era NaN y NaN pasado a uint8 da ~0 en toda la figura.
        sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'portrait_gen'))
        import colour
        a = np.zeros((1024, 768, 4), np.uint8)
        a[100:960, 250:520] = (60, 110, 50, 255)     # Solo ropa verde: ningún píxel de piel
        target = {'L': 70.0, 'a': 15.0, 'b': 25.0, 'sa': 5.0, 'sb': 5.0}
        out = colour.correct(a, target=target)
        self.assertTrue(np.array_equal(out, a), 'sin piel que medir, no se corrige nada')



class HeadCropTests(unittest.TestCase):
    def test_el_recorte_de_cabeza_es_cuadrado_y_contiene_la_cara(self):
        # LoRA v2 (sesión B): en una figura entera de 1024 la cara mide ~60 px y el LoRA no aprende los ojos de los
        # originales (prueba ciega: "ojos pequeños", "cara emborronada"). Se añaden recortes de la cabeza ampliados.
        sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'portrait_gen'))
        import lora_dataset
        a = np.zeros((1024, 768, 4), np.uint8)
        a[100:300, 330:430] = (225, 160, 110, 255)   # Cabeza
        a[300:960, 250:520] = (60, 110, 50, 255)     # Cuerpo
        box = lora_dataset.head_box(Image.fromarray(a, 'RGBA'))
        left, top, right, bottom = box
        self.assertEqual(right - left, bottom - top, 'cuadrado')
        self.assertLessEqual(top, 100)
        self.assertGreaterEqual(bottom, 300, 'entra la cabeza entera')
        self.assertLessEqual(left, 330)
        self.assertGreaterEqual(right, 430)
        self.assertLess(bottom - top, 500, 'es la cabeza, no la figura')



class FaceFixTests(unittest.TestCase):
    def test_un_recuadro_sin_cara_se_rechaza(self):
        # Revisión independiente: si la máscara de la figura falla, el recuadro cae en el fondo y la cara no se repinta
        # sin avisar. Un recuadro sin piel no es una cabeza.
        sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'portrait_gen'))
        import facefix
        grey = Image.new('RGB', (864, 1152), (90, 90, 96))
        self.assertFalse(facefix.looks_like_head(grey, (300, 0, 600, 300)))
        face = Image.new('RGB', (864, 1152), (90, 90, 96))
        face.paste((225, 160, 110), (380, 80, 520, 240))
        self.assertTrue(facefix.looks_like_head(face, (300, 40, 600, 340)))
        # Caso real (8700, 8702): el LoRA v2 pinta la piel muy saturada y la máscara de piel no la veía
        vivid = Image.new('RGB', (864, 1152), (90, 90, 96))
        vivid.paste((255, 150, 40), (380, 80, 520, 240))
        self.assertTrue(facefix.looks_like_head(vivid, (300, 40, 600, 340)))

    def test_pegar_la_cara_no_toca_nada_fuera_del_recuadro(self):
        # facefix.py: la cara se repinta aparte a 1024 y se pega con borde suave. Fuera del recuadro, ni un píxel cambia.
        sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'portrait_gen'))
        import facefix
        base = Image.new('RGB', (864, 1152), (10, 20, 30))
        face = Image.new('RGB', (1024, 1024), (200, 150, 100))
        box = (300, 100, 500, 300)
        out = np.asarray(facefix.paste_back(base, face, box))
        self.assertTrue((out[50, 50] == (10, 20, 30)).all(), 'fuera, igual')
        self.assertTrue((out[99, 400] == (10, 20, 30)).all(), 'justo encima, igual')
        self.assertTrue((out[200, 400] == (200, 150, 100)).all(), 'el centro es la cara nueva')
        mid = out[101, 400].astype(int)
        self.assertTrue(10 <= mid[0] <= 200, 'el borde mezcla')



class SeloutTests(unittest.TestCase):
    def test_el_contorno_toma_el_tono_oscuro_de_cada_zona(self):
        # Prueba ciega 10 (2 de 3, motivo común): "contorno negro grueso y uniforme; los originales usan contornos más
        # finos que cambian de color con la zona". Con selout, el borde junto a la camisa verde es verde oscuro y el
        # borde junto a la cara, marrón oscuro; nunca negro puro.
        a = np.zeros((400, 300, 4), np.uint8)
        a[50:150, 100:200] = (230, 170, 120, 255)    # Cara
        a[150:350, 75:225] = (70, 130, 60, 255)      # Camisa
        out = np.asarray(style.reinforce_outline(Image.fromarray(a, 'RGBA'), cell=5, cells=1, selout=True)).astype(int)
        shirt_edge = out[250, 77, :3]
        face_edge = out[52, 150, :3]
        self.assertGreater(shirt_edge[1], shirt_edge[0], f'verde oscuro, no negro: {shirt_edge}')
        self.assertLess(shirt_edge.max(), 90, 'pero oscuro')
        self.assertGreater(face_edge[0], face_edge[2], f'marrón oscuro: {face_edge}')
        self.assertTrue((out[250, 150, :3] == (70, 130, 60)).all(), 'el interior no cambia')


if __name__ == '__main__':
    unittest.main()
