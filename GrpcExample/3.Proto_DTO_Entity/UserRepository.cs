using Proto_DTO_Entity.Models;

namespace Proto_DTO_Entity;

public class UserRepository
{
    private readonly Dictionary<int, User> _users = new();
    private int _nextId = 1;

    public User Create(string name, int age)
    {
        var user = new User { Id = _nextId++, Name = name, Age = age };
        _users[user.Id] = user;
        return user;
    }

    public User? Get(int id) => _users.TryGetValue(id, out var user) ? user : null;

    public User? Update(int id, string name, int age)
    {
        if (!_users.ContainsKey(id)) return null;
        var user = _users[id];
        user.Name = name;
        user.Age = age;
        return user;
    }

    public bool Delete(int id) => _users.Remove(id);
}