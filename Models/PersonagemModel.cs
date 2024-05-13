using System;

public class PersonagemModel
{
    public string Id { get; set; } // No frontend, o Id pode ser uma string
    public string Nome { get; set; }
    public string Observacao { get; set; }
    public TipoPersonagem? TipoPersonagem { get; set; }
}