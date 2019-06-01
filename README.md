# standard-repository

this project is an simple alternative for entity framework...

with StandardRepository;

you get record revisions.

    Task<List<EntityRevision<T>>> SelectRevisions(long id);
    Task<bool> RestoreRevision(long currentUserId, long id, int revision);

and also soft delete logic.

    Task<bool> Delete(long currentUserId, long id);
    Task<bool> UndoDelete(long currentUserId, long id);
    Task<bool> HardDelete(long currentUserId, long id);