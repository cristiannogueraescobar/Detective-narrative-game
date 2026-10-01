# Casos reales detrás de las historias (documento de diseño, no va en la build)

**Criterio ético.** Las historias son ficción. De los casos reales se toman **patrones** (cómo se sostiene una
mentira, qué detalles delatan, cómo se rompe una coartada), no nombres, ni detalles que identifiquen a las víctimas
—niñas y jóvenes con familias vivas— ni afirmaciones sin contrastar sobre personas reales. Investigación hecha por
un subagente con búsqueda web el 30 sep 2026 (algunos grandes periódicos estaban bloqueados para la herramienta; se
usaron agencias, prensa regional y jurídica). Lo que solo aparecía en una fuente va marcado como "según una fuente".

## Qué caso inspira cada historia

| Historia | Caso real más probable | Fuentes |
|---|---|---|
| 1 · La hija perfecta | Caso Asunta (Santiago/Teo, 2013): niña adoptada de 12 años sedada con lorazepam; los dos padres condenados. | [Wikipedia](https://en.wikipedia.org/wiki/The_Asunta_Case) · [Infobae 2024](https://www.infobae.com/espana/2024/04/26/los-cabos-sueltos-de-la-sentencia-del-caso-asunta-del-movil-del-crimen-al-papel-de-alfonso-basterra-en-la-ejecucion/) · [The Local 2015](https://www.thelocal.es/20151001/spanish-mother-testifies-over-killing-of-adopted-chinese-girl) · [Orain/EFE](https://orain.eus/es/actualidad/sociedad/2015/10/01/crimen-asunta-basterra--declaran-juicio-padres/) |
| 2 · Noche de verano | Caso Diana Quer (A Pobra do Caramiñal, 2016): joven que volvía sola de las fiestas; último WhatsApp; móvil hallado en la ría. | [Vozpópuli](https://www.vozpopuli.com/espana/cronologia-caso-diana-quer-muerte-desaparicion-el-chicle_0_1095490541.html) · [Xataka](https://www.xataka.com/moviles/asi-es-como-la-policia-del-siglo-xxi-situa-al-sospechoso-en-la-escena-del-crimen) · [Público](https://www.publico.es/sociedad/exmujer-chicle-juicio-asesinato-diana-quer-evidentemente-creo.html) |
| 3 · Humo y silencio | Caso Bretón (Córdoba, 2011): padre que, en plena separación, mató a sus dos hijos y los quemó en una hoguera de la finca familiar. | [Legal Today](https://www.legaltoday.com/actualidad-juridica/noticias-de-derecho/breton-condenado-a-40-anos-de-carcel-2013-07-22/) · [Orain](https://orain.eus/es/actualidad/sociedad/2012/08/27/paco-etxeberria--paco-etxeberria-da-vuelco-al-caso-de-ruth-y-jose/) · [La Prensa/EFE](https://www.laprensa.hn/mundo/espanol-culpable-de-asesinar-y-quemar-a-sus-hijos-HALP363062) · [Periódico de Ibiza/EFE](https://www.periodicodeibiza.es/sucesos/ultimas/2013/07/01/102799/policia-sombra-breton-dice-estaba-jovial-mientras-inspeccionaba-hoguera.html) |

### ⚠ Para decidir (Cristian)
- **La historia 1 está muy cerca del caso real** (Santiago + niña adoptada de 12 años + septiembre + fármaco para
  dormir + padre abogado). **La vecina se llamaba "Rosario", el nombre de pila de la madre condenada** en ese caso: **renombrada a Amparo en la Sesión A** (1-10-2026; la clave interna del arte, `rosario`, no la ve nadie).
  No lo he cambiado (es decisión tuya), pero propongo renombrar a la vecina (p. ej. "Remedios") y alejar algún
  detalle identificativo (ciudad o edad).
- Las historias 2 y 3 ya se alejan lo suficiente (lugares ficticios, otras edades, otros culpables; la variante
  3B, la madre que huye con la niña, es invención).

## Patrones que dan credibilidad (y dónde se usaron)

**Cómo sostiene la mentira el culpable**
- Controla la línea temporal: un "descubrimiento" tardío o escenificado y luego la llamada o la denuncia.
- Dirige el relato hacia un accidente, una huida, un desconocido o la expareja.
- Siembra explicaciones inocentes antes ("dormía mal", "estaba enferma", "el humo eran rastrojos").
- Se ofrece a buscar o a coordinar, para estar cerca de la investigación.
- Cede **solo lo que una prueba concreta demuestra** (en el juego: la versión B al enseñar la prueba).
→ Usado en el campo *cómo sostiene la mentira* de cada culpable (StoriesDatabase.json).

**Qué delata**
- Precisión excesiva con las horas y correcciones espontáneas de detalles menores.
- Limpieza o lavado fuera de lo normal; romper una rutina de años (fichar tarde, lavar una furgoneta).
- La versión cambia cada vez que aparece una prueba.
→ Usado en los *gestos al mentir* y en pistas ya existentes (lejía en 1C, manguera en 2B, fichaje en 2B).

**Cómo se resuelven**
- Aparatos imparciales: cámaras, antenas, GPS, fichajes, recetas de farmacia; toxicología o análisis del fuego;
  y el pequeño detalle de un vecino.
- Una primera conclusión oficial puede estar equivocada y una segunda mirada le da la vuelta.
→ Coincide con el diseño de pistas (GPS, fichaje, cámara, recetas, ventana de la vecina).

**Inocentes bajo sospecha**
- Se ponen a la defensiva y leen las preguntas como acusaciones.
- Ocultan secretos ajenos al crimen (alcohol, un lío, dinero) y parecen culpables por eso.
- Se equivocan de buena fe con las horas; sienten culpa por no haber hecho algo.
- Cuando dicen la verdad: dan datos comprobables y admiten "no lo sé".
→ Usado en los *gestos al decir la verdad* y en la *progresión bajo presión* de cada personaje.
