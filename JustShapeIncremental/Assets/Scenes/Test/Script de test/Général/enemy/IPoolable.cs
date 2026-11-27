// les scripts implémentant IPoolable doivent être sur la racine du gameObject
public interface IPoolable
{
    void ReturnToPool();
}