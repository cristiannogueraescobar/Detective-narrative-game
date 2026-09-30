/// <summary>
/// ¿Está la respuesta en otro alfabeto? qwen (entrenado sobre todo en chino e inglés) a veces se pasa al chino y,
/// con eso en el historial, sigue así toda la partida (visto con el bot, día 3). Cuenta caracteres de escrituras
/// que el español nunca usa: chino, japonés, coreano, cirílico, árabe.
/// </summary>
public static class LanguageCheck
{
    private const int Threshold = 2; // Un carácter suelto puede ser un emoji o un símbolo raro

    public static bool IsForeign(string text) => ForeignCount(text) >= Threshold;

    public static int ForeignCount(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;
        int count = 0;
        foreach (char c in text)
        {
            if ((c >= '一' && c <= '鿿')      // Chino (unificado)
                || (c >= '぀' && c <= 'ヿ')   // Hiragana y katakana
                || (c >= '가' && c <= '힯')   // Hangul
                || (c >= 'Ѐ' && c <= 'ӿ')   // Cirílico
                || (c >= '؀' && c <= 'ۿ')   // Árabe
                || (c >= '　' && c <= '〿')   // Puntuación CJK (「」。、)
                || (c >= '＀' && c <= '￯'))  // Formas de ancho completo (？，)
                count++;
        }
        return count;
    }
}
