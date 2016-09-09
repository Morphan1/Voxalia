namespace Voxalia.ServerGame.EntitySystem
{
    public interface EntityDamageable
    {
        double GetHealth();

        double GetMaxHealth();

        void SetHealth(double health);

        void SetMaxHealth(double health);

        void Damage(double amount);
    }
}
