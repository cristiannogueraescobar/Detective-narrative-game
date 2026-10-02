docs/screenshots/2026-10-02/galeria/propuesta-resumen-acusacion.jpg

# Propuesta: resumen de pruebas en el hueco de la rueda (solo capturas, no integrada)

Rama `feature/propuesta-resumen-acusacion`, que no se fusiona. La tarjeta solo existe en la prueba de captura
(`AnimationCapture.PropuestaResumenAcusacion`); el juego no la tiene.

En 20:9 (1080 × 2400 y 1440 × 3200) la rueda deja pared vacía por encima de la raya de 2 m. La propuesta pone ahí una
tarjeta "TUS PRUEBAS" entre el texto de la acusación y esa raya. Lleva las pistas de la libreta por su nombre y las
contradicciones como "Una versión choca con: …". Con la libreta vacía, una línea que dice que acusar es una apuesta y
que se puede volver.

Capturas: historia 2, primer día. "Varias pistas" son las cuatro primeras de la variante y las contradicciones que el
juego calcula con ellas. La variante cambia en cada partida: por eso las pistas no son las mismas a 1080 × 2400 que a
1440 × 3200.

## Lo que hay que saber antes de decidir

- En la captura con pistas, el texto de arriba dice "cero pistas". No es un fallo del juego: esa frase cuenta las
  opciones del desplegable de pruebas, que el juego rellena al descubrir cada pista jugando. La captura mete las pistas
  directamente en el estado, sin ese paso. Jugando diría "cuatro pistas".
- Si se integra, el texto de arriba ("En tu libreta: …") repetiría lo que dice la tarjeta: habría que quitar uno de los
  dos.
- En 16:9 no hay hueco: la rueda ya llena la pantalla. La tarjeta tendría que salir solo cuando sobre sitio o ser
  plegable.
- Las pistas salen por su nombre, sin decir a quién incriminan. El juego ya evita eso en la acusación y lo mantengo.
- Con muchas pistas (6 o más, más contradicciones), la tarjeta reduce el texto hasta el mínimo legible. Si aún no cabe,
  haría falta desplazamiento o solo las cifras.
