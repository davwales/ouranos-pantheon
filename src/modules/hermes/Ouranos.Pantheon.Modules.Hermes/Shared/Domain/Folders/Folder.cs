using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;

public sealed class Folder : BaseEntity<Id<Folder>>
{
    public const int MaxDepth = 5;
    private const string DefaultName = "New Folder";

    private Folder(Id<Folder> id)
        : base(id) { }

    public string Name
    {
        get;
        private set => field = string.IsNullOrWhiteSpace(value) ? DefaultName : value.Trim();
    } = string.Empty;

    public Id<Folder>? ParentFolderId { get; private set; }

    public bool IsPublic { get; private set; } = true;

    public Folder? ParentFolder { get; private set; }

    public List<Folder> ChildFolders { get; private set; } = [];

    public List<Conversation> Conversations { get; private set; } = [];

    public static Folder Create(
        Id<Folder> id,
        string name,
        bool isPublic = true,
        Id<Folder>? parentFolderId = null
    )
    {
        return new Folder(id)
        {
            Name = name,
            IsPublic = isPublic,
            ParentFolderId = parentFolderId,
        };
    }

    public void Update(string name, bool isPublic, Id<Folder>? parentFolderId)
    {
        Name = name;
        IsPublic = isPublic;
        ParentFolderId = parentFolderId;

        Update();
    }
}
