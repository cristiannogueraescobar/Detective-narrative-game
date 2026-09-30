using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;

/// <summary>
/// Chat con burbujas sobre la escena real (LayoutPreview): lados, anchos, que el texto quepa y el pool.
/// </summary>
public class ChatViewTests
{
    private LayoutPreview.Session session;
    private ChatView chat;

    [SetUp]
    public void SetUp()
    {
        session = LayoutPreview.Open(new Vector2(1080f, 1920f));
        LayoutPreview.ShowOnly(session, "InterrogationPanel");
        chat = LayoutPreview.Find(session, "ConversationScroll").GetComponent<ChatView>();
    }

    [TearDown]
    public void TearDown()
    {
        LayoutPreview.Close();
    }

    private void Show(params ChatEntry[] entries)
    {
        session.ui.ShowWaiting(false);
        chat.Show(entries.ToList());
        LayoutPreview.Rebuild((RectTransform)session.canvas.transform);
    }

    private List<RectTransform> Bubbles()
    {
        return chat.GetComponentsInChildren<RectTransform>(false).Where(r => r.name == "Globo" && r.parent.name == "Burbuja (auto)").ToList();
    }

    private static Rect World(RectTransform rect)
    {
        var corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        return new Rect(corners[0].x, corners[0].y, corners[2].x - corners[0].x, corners[2].y - corners[0].y);
    }

    [Test]
    public void JugadorALaDerechaYSospechosoALaIzquierda()
    {
        Show(ChatEntry.Player("¿Dónde estaba?", null, "09:00"), ChatEntry.Suspect("CARMEN", "En casa.", "09:00"));

        List<RectTransform> bubbles = Bubbles();
        Rect row = World((RectTransform)bubbles[0].parent);
        Rect player = World(bubbles[0]);
        Rect suspect = World(bubbles[1]);

        Assert.AreEqual(row.xMax, player.xMax, 2f, "la del jugador pegada a la derecha");
        Assert.Greater(suspect.xMin, row.xMin + ChatView.AvatarSize - 2f, "la del sospechoso deja sitio al retrato");
        Assert.Less(suspect.xMax, row.center.x + row.width * 0.2f, "una respuesta corta no ocupa la fila");
    }

    [Test]
    public void LaBurbujaSeAjustaAlTextoConUnMaximo()
    {
        string longText = string.Join(" ", Enumerable.Repeat("una respuesta muy larga", 30));
        Show(ChatEntry.Suspect("CARMEN", "No.", "09:00"), ChatEntry.Suspect("CARMEN", longText, "09:00"));

        List<RectTransform> bubbles = Bubbles();
        float rowWidth = ((RectTransform)bubbles[0].parent).rect.width;

        Assert.Less(bubbles[0].rect.width, bubbles[1].rect.width, "la corta es más estrecha");
        Assert.LessOrEqual(bubbles[1].rect.width, rowWidth * ChatView.MaxBubbleFraction + 2f, "máximo del 80 %");
    }

    [Test]
    public void ElTextoCabeEnSuBurbuja()
    {
        string longText = string.Join(" ", Enumerable.Repeat("palabra", 120));
        Show(ChatEntry.Player(longText, "Lo que vio la ventana", "09:00"), ChatEntry.Suspect("CARMEN", longText, "09:00"));

        foreach (RectTransform bubble in Bubbles())
        {
            Rect outer = World(bubble);
            foreach (TMP_Text text in bubble.GetComponentsInChildren<TMP_Text>(false))
            {
                text.ForceMeshUpdate(true, true);
                Rect inner = World(text.rectTransform);
                Assert.GreaterOrEqual(inner.xMin, outer.xMin - 1f, text.name);
                Assert.LessOrEqual(inner.xMax, outer.xMax + 1f, text.name);
                Assert.GreaterOrEqual(inner.yMin, outer.yMin - 1f, text.name);
                Assert.GreaterOrEqual(text.rectTransform.rect.height + 1f, text.preferredHeight, $"{text.name}: alto suficiente");
            }
        }
    }

    [Test]
    public void LoQueEscribeElJugadorNoSeInterpretaComoFormato()
    {
        Show(ChatEntry.Player("<size=300>hola</size>", null, "09:00"));

        TMP_Text body = Bubbles()[0].GetComponentsInChildren<TMP_Text>().First(t => t.name == "Texto");
        Assert.IsFalse(body.richText);
    }

    [Test]
    public void CambiarDeConversacionReutilizaLasFilas()
    {
        var a = new List<ChatEntry>();
        var b = new List<ChatEntry>();
        for (int i = 0; i < 10; i++)
        {
            a.Add(ChatEntry.Player("pregunta " + i, null, "09:00"));
            a.Add(ChatEntry.Suspect("A", "respuesta " + i, "09:00"));
            b.Add(ChatEntry.Suspect("B", "otra " + i, "09:00"));
        }

        chat.Show(a);
        chat.Show(b);
        int created = chat.CreatedRows;
        for (int i = 0; i < 5; i++)
        {
            chat.Show(a);
            chat.Show(b);
        }

        Assert.AreEqual(created, chat.CreatedRows, "ir y volver no crea filas nuevas");
        Assert.AreEqual(b.Count, chat.RowCount);
    }

    [Test]
    public void AnadirSoloPintaLoNuevo()
    {
        var entries = new List<ChatEntry> { ChatEntry.Player("uno", null, "09:00") };
        chat.Show(entries);
        GameObject first = Bubbles()[0].parent.gameObject;
        TMP_Text firstText = first.GetComponentsInChildren<TMP_Text>().First(t => t.name == "Texto");

        entries.Add(ChatEntry.Suspect("A", "dos", "09:00"));
        chat.Show(entries);

        Assert.AreEqual(2, chat.RowCount);
        Assert.AreSame(first, Bubbles()[0].parent.gameObject, "la primera fila sigue siendo la misma");
        Assert.AreEqual("uno", firstText.text);
    }

    [Test]
    public void EscribiendoMuestraPuntosNoEtiquetas()
    {
        session.ui.ShowWaiting(true);
        LayoutPreview.Rebuild((RectTransform)session.canvas.transform);

        Transform typing = chat.GetComponentsInChildren<Transform>(false).First(t => t.name == "Escribiendo (auto)");
        TMP_Text body = typing.GetComponentsInChildren<TMP_Text>().First(t => t.name == "Texto");
        body.ForceMeshUpdate(true, true);
        string visible = new string(body.textInfo.characterInfo.Take(body.textInfo.characterCount).Select(c => c.character).ToArray());

        StringAssert.DoesNotContain("<", visible);
        StringAssert.Contains("•", visible);
    }

    [Test]
    public void AvisosCentradosSinBurbuja()
    {
        Show(ChatEntry.Day(2, "El forense habla."), ChatEntry.System(ChatEntryKind.Unlock, "Marcos"));

        Assert.IsEmpty(Bubbles());
        Assert.AreEqual(2, chat.RowCount);
    }

    [Test]
    public void UnCasoNuevoEmpiezaSinRestosDelAnterior()
    {
        Assert.IsFalse(session.ui.Conversations.IsEmpty, "la vista previa trae una conversación larga");
        session.ui.ResetForNewCase();
        Assert.IsTrue(session.ui.Conversations.IsEmpty);
        Assert.IsNull(session.ui.CurrentSuspectId);
        Assert.AreEqual(0, chat.RowCount);
    }
}
