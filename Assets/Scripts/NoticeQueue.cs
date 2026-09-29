using System;
using System.Collections.Generic;

/// <summary>
/// Avisos de la UI (pistas, contradicciones, sospechosos nuevos) que pueden retrasarse hasta después
/// de mostrar la respuesta que los provocó.
/// </summary>
public class NoticeQueue
{
    private readonly List<Action> pending = new List<Action>();
    private bool deferring;

    public void BeginDefer()
    {
        deferring = true;
    }

    public void Post(Action notice)
    {
        if (deferring)
            pending.Add(notice);
        else
            notice();
    }

    /// <summary>
    /// Ejecuta los avisos pendientes en orden y vuelve al modo inmediato.
    /// </summary>
    public void Flush()
    {
        deferring = false;
        var toRun = new List<Action>(pending);
        pending.Clear();

        // Un aviso que falla no debe llevarse por delante a los demás
        foreach (Action notice in toRun)
        {
            try
            {
                notice();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }
    }

    /// <summary>
    /// Descarta los avisos pendientes (petición fallida) y vuelve al modo inmediato.
    /// </summary>
    public void Discard()
    {
        deferring = false;
        pending.Clear();
    }
}
