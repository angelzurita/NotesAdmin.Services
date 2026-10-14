namespace AdminServices.IntegrationTests;

public sealed record Api<T>(bool Success, string Message, T? Data, List<string> Errors);
public sealed record NoteItem(Guid Id, string Title, string Content, Guid? CategoryId);
public sealed record CategoryItem(Guid Id, string Name);
public sealed record FolderContents(Guid? FolderId, string FolderName, List<FolderItem> SubFolders, List<DocumentItem> Documents);
public sealed record FolderItem(Guid Id, string Name);
public sealed record DocumentItem(Guid Id, string FileName);
public sealed record Metas(List<string> Dias, int Meta);
