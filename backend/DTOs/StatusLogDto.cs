public class CreateStatusLogDto
{
    public int TaskItemId { get; set; }
    public int? RunnerId { get; set; }
    public string Comment { get; set; } = null!;
}

public class UpdateStatusLogDto
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public int? RunnerId { get; set; }
    public string Comment { get; set; } = null!;
}