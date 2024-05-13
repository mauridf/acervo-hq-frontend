using System;

public class HqModel
{
    public string Id { get; set; }

    public String Titulo { get; set; }

    public String? TituloOriginal { get; set; }

    public Categoria Categoria { get; set; }

    public String? TotalEdicoes { get; set; }

    public Status Status { get; set; }

    public String? Observacao { get; set; }

    public List<string> Edicoes { get; set; }

    public List<string> Editoras { get; set; }

    public List<string> Personagens { get; set; }
}