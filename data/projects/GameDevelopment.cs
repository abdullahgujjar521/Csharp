using System;
using System.Collections.Generic;
using System.Linq;

namespace GameProject
{
    public enum CharacterClass
    {
        Warrior,
        Mage,
        Rogue
    }
    
    public enum EnemyType
    {
        Goblin,
        Orc,
        Dragon
    }
    
    public enum ItemType
    {
        Weapon,
        Potion,
        Armor,
        Misc
    }
    
    public enum WeaponType
    {
        Sword,
        Staff,
        Dagger
    }
    
    public enum PotionType
    {
        Health,
        Mana
    }
    
    public enum CombatResult
    {
        Victory,
        Defeat,
        Escape
    }
    
    public abstract class Character
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Speed { get; set; }
        public int Experience { get; set; }
        public int Gold { get; set; }
        public List<Item> Inventory { get; set; } = new List<Item>();
        public Weapon EquippedWeapon { get; set; }
        
        public bool IsAlive => Health > 0;
        
        public virtual int CalculateDamage()
        {
            int baseDamage = Attack;
            if (EquippedWeapon != null)
            {
                baseDamage += EquippedWeapon.Damage;
            }
            return baseDamage + new Random().Next(-2, 3);
        }
        
        public virtual void TakeDamage(int damage)
        {
            int actualDamage = Math.Max(1, damage - Defense);
            Health -= actualDamage;
            Console.WriteLine($"{Name} takes {actualDamage} damage! Health: {Health}/{MaxHealth}");
        }
        
        public virtual void Heal(int amount)
        {
            Health = Math.Min(MaxHealth, Health + amount);
            Console.WriteLine($"{Name} heals {amount} HP! Health: {Health}/{MaxHealth}");
        }
        
        public virtual bool GainExperience(int exp)
        {
            Experience += exp;
            Console.WriteLine($"{Name} gains {exp} experience!");
            
            int expNeeded = Level * 100;
            if (Experience >= expNeeded)
            {
                LevelUp();
                return true;
            }
            return false;
        }
        
        protected virtual void LevelUp()
        {
            Level++;
            Experience -= Level * 100;
            MaxHealth += 10;
            Health = MaxHealth;
            Attack += 2;
            Defense += 1;
            Speed += 1;
            Console.WriteLine($"{Name} leveled up to {Level}!");
        }
        
        public override string ToString()
        {
            return $"{Name} (Level {Level}) - HP: {Health}/{MaxHealth} - ATK: {Attack} - DEF: {Defense}";
        }
    }
    
    public class Player : Character
    {
        public CharacterClass CharacterClass { get; set; }
        public int Mana { get; set; }
        public int MaxMana { get; set; }
        
        public Player(string name, CharacterClass characterClass)
        {
            Name = name;
            CharacterClass = characterClass;
            Level = 1;
            InitializeStats();
        }
        
        private void InitializeStats()
        {
            switch (CharacterClass)
            {
                case CharacterClass.Warrior:
                    MaxHealth = 120;
                    Health = 120;
                    Attack = 15;
                    Defense = 8;
                    Speed = 5;
                    MaxMana = 20;
                    Mana = 20;
                    break;
                case CharacterClass.Mage:
                    MaxHealth = 80;
                    Health = 80;
                    Attack = 10;
                    Defense = 4;
                    Speed = 6;
                    MaxMana = 100;
                    Mana = 100;
                    break;
                case CharacterClass.Rogue:
                    MaxHealth = 100;
                    Health = 100;
                    Attack = 12;
                    Defense = 6;
                    Speed = 10;
                    MaxMana = 40;
                    Mana = 40;
                    break;
            }
        }
        
        public bool UseMana(int amount)
        {
            if (Mana >= amount)
            {
                Mana -= amount;
                return true;
            }
            Console.WriteLine("Not enough mana!");
            return false;
        }
        
        public void Rest()
        {
            Health = MaxHealth;
            Mana = MaxMana;
            Console.WriteLine($"{Name} rests and recovers fully!");
        }
        
        protected override void LevelUp()
        {
            base.LevelUp();
            
            switch (CharacterClass)
            {
                case CharacterClass.Warrior:
                    MaxHealth += 15;
                    Attack += 3;
                    Defense += 2;
                    break;
                case CharacterClass.Mage:
                    MaxHealth += 8;
                    MaxMana += 20;
                    Attack += 2;
                    break;
                case CharacterClass.Rogue:
                    MaxHealth += 10;
                    Speed += 2;
                    Attack += 3;
                    break;
            }
            
            Health = MaxHealth;
            Mana = MaxMana;
        }
    }
    
    public class Enemy : Character
    {
        public EnemyType EnemyType { get; set; }
        public int ExperienceReward { get; set; }
        public int GoldReward { get; set; }
        public List<Item> LootTable { get; set; } = new List<Item>();
        
        public Enemy(string name, EnemyType enemyType, int level)
        {
            Name = name;
            EnemyType = enemyType;
            Level = level;
            InitializeStats();
        }
        
        private void InitializeStats()
        {
            switch (EnemyType)
            {
                case EnemyType.Goblin:
                    MaxHealth = 30 + (Level * 10);
                    Health = MaxHealth;
                    Attack = 5 + (Level * 2);
                    Defense = 2 + Level;
                    Speed = 8;
                    ExperienceReward = 20 + (Level * 10);
                    GoldReward = 10 + (Level * 5);
                    break;
                case EnemyType.Orc:
                    MaxHealth = 50 + (Level * 15);
                    Health = MaxHealth;
                    Attack = 8 + (Level * 3);
                    Defense = 5 + (Level * 2);
                    Speed = 4;
                    ExperienceReward = 40 + (Level * 15);
                    GoldReward = 20 + (Level * 10);
                    break;
                case EnemyType.Dragon:
                    MaxHealth = 100 + (Level * 30);
                    Health = MaxHealth;
                    Attack = 15 + (Level * 5);
                    Defense = 10 + (Level * 3);
                    Speed = 6;
                    ExperienceReward = 100 + (Level * 50);
                    GoldReward = 50 + (Level * 25);
                    break;
            }
        }
        
        public Item DropLoot()
        {
            if (LootTable.Any() && new Random().NextDouble() < 0.5)
            {
                return LootTable[new Random().Next(LootTable.Count)];
            }
            return null;
        }
    }
    
    public abstract class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Value { get; set; }
        public ItemType ItemType { get; set; }
        public int Quantity { get; set; } = 1;
        
        public abstract void Use(Character character);
        
        public override string ToString()
        {
            return Quantity > 1 ? $"{Name} x{Quantity}" : Name;
        }
    }
    
    public class Weapon : Item
    {
        public int Damage { get; set; }
        public WeaponType WeaponType { get; set; }
        
        public Weapon()
        {
            ItemType = ItemType.Weapon;
        }
        
        public override void Use(Character character)
        {
            if (character is Player player)
            {
                player.EquippedWeapon = this;
                Console.WriteLine($"{player.Name} equipped {Name}!");
            }
        }
    }
    
    public class Potion : Item
    {
        public int HealAmount { get; set; }
        public PotionType PotionType { get; set; }
        
        public Potion()
        {
            ItemType = ItemType.Potion;
        }
        
        public override void Use(Character character)
        {
            switch (PotionType)
            {
                case PotionType.Health:
                    character.Heal(HealAmount);
                    Console.WriteLine($"{character.Name} used {Name} and healed {HealAmount} HP!");
                    break;
                case PotionType.Mana:
                    if (character is Player player)
                    {
                        player.Mana = Math.Min(player.MaxMana, player.Mana + HealAmount);
                        Console.WriteLine($"{player.Name} used {Name} and restored {HealAmount} MP!");
                    }
                    break;
            }
            Quantity--;
        }
    }
    
    public class CombatSystem
    {
        private readonly Random random = new Random();
        
        public CombatResult StartCombat(Player player, Enemy enemy)
        {
            Console.WriteLine($"\n=== COMBAT ===");
            Console.WriteLine($"{player.Name} vs {enemy.Name}!");
            Console.WriteLine("================");
            
            while (player.IsAlive && enemy.IsAlive)
            {
                DisplayCombatStatus(player, enemy);
                DisplayCombatOptions();
                
                string choice = Console.ReadLine();
                ProcessCombatChoice(player, enemy, choice);
                
                if (enemy.IsAlive)
                {
                    EnemyAttack(enemy, player);
                }
            }
            
            return DetermineCombatResult(player, enemy);
        }
        
        private void DisplayCombatStatus(Player player, Enemy enemy)
        {
            Console.WriteLine($"\n{player.Name}: HP {player.Health}/{player.MaxHealth} | MP {player.Mana}/{player.MaxMana}");
            Console.WriteLine($"{enemy.Name}: HP {enemy.Health}/{enemy.MaxHealth}");
        }
        
        private void DisplayCombatOptions()
        {
            Console.WriteLine("\nCombat Options:");
            Console.WriteLine("1. Attack");
            Console.WriteLine("2. Use Skill");
            Console.WriteLine("3. Use Item");
            Console.WriteLine("4. Run");
            Console.Write("Choose your action: ");
        }
        
        private void ProcessCombatChoice(Player player, Enemy enemy, string choice)
        {
            switch (choice)
            {
                case "1":
                    PlayerAttack(player, enemy);
                    break;
                case "2":
                    UseSkill(player, enemy);
                    break;
                case "3":
                    UseItem(player);
                    break;
                case "4":
                    if (TryRun(player, enemy))
                    {
                        Console.WriteLine($"{player.Name} ran away from combat!");
                    }
                    break;
                default:
                    Console.WriteLine("Invalid choice!");
                    break;
            }
        }
        
        public void PlayerAttack(Player player, Enemy enemy)
        {
            int damage = player.CalculateDamage();
            enemy.TakeDamage(damage);
            Console.WriteLine($"{player.Name} attacks {enemy.Name} for {damage} damage!");
        }
        
        public void EnemyAttack(Enemy enemy, Player player)
        {
            System.Threading.Thread.Sleep(1000);
            int damage = enemy.CalculateDamage();
            player.TakeDamage(damage);
            Console.WriteLine($"{enemy.Name} attacks {player.Name} for {damage} damage!");
        }
        
        public bool UseSkill(Player player, Enemy enemy)
        {
            switch (player.CharacterClass)
            {
                case CharacterClass.Warrior:
                    return UseWarriorSkill(player, enemy);
                case CharacterClass.Mage:
                    return UseMageSkill(player, enemy);
                case CharacterClass.Rogue:
                    return UseRogueSkill(player, enemy);
            }
            return false;
        }
        
        private bool UseWarriorSkill(Player player, Enemy enemy)
        {
            Console.WriteLine("Warrior Skills:");
            Console.WriteLine("1. Power Strike (5 MP) - Double damage");
            Console.Write("Choose skill: ");
            string choice = Console.ReadLine();
            
            if (choice == "1" && player.UseMana(5))
            {
                int damage = player.CalculateDamage() * 2;
                enemy.TakeDamage(damage);
                Console.WriteLine($"{player.Name} uses Power Strike for {damage} damage!");
                return true;
            }
            return false;
        }
        
        private bool UseMageSkill(Player player, Enemy enemy)
        {
            Console.WriteLine("Mage Skills:");
            Console.WriteLine("1. Fireball (10 MP) - 15 damage");
            Console.WriteLine("2. Heal (8 MP) - Restore 20 HP");
            Console.Write("Choose skill: ");
            string choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    if (player.UseMana(10))
                    {
                        enemy.TakeDamage(15);
                        Console.WriteLine($"{player.Name} casts Fireball for 15 damage!");
                        return true;
                    }
                    break;
                case "2":
                    if (player.UseMana(8))
                    {
                        player.Heal(20);
                        return true;
                    }
                    break;
            }
            return false;
        }
        
        private bool UseRogueSkill(Player player, Enemy enemy)
        {
            Console.WriteLine("Rogue Skills:");
            Console.WriteLine("1. Backstab (6 MP) - Triple damage");
            Console.Write("Choose skill: ");
            string choice = Console.ReadLine();
            
            if (choice == "1" && player.UseMana(6))
            {
                int damage = player.CalculateDamage() * 3;
                enemy.TakeDamage(damage);
                Console.WriteLine($"{player.Name} uses Backstab for {damage} damage!");
                return true;
            }
            return false;
        }
        
        private void UseItem(Player player)
        {
            var potions = player.Inventory.OfType<Potion>().ToList();
            if (!potions.Any())
            {
                Console.WriteLine("No potions available!");
                return;
            }
            
            Console.WriteLine("Available potions:");
            for (int i = 0; i < potions.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {potions[i]}");
            }
            
            Console.Write("Choose potion: ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= potions.Count)
            {
                potions[choice - 1].Use(player);
                if (potions[choice - 1].Quantity <= 0)
                {
                    player.Inventory.Remove(potions[choice - 1]);
                }
            }
        }
        
        private bool TryRun(Player player, Enemy enemy)
        {
            double runChance = (double)player.Speed / (player.Speed + enemy.Speed) * 0.8;
            return random.NextDouble() < runChance;
        }
        
        private CombatResult DetermineCombatResult(Player player, Enemy enemy)
        {
            if (!player.IsAlive)
            {
                return CombatResult.Defeat;
            }
            
            if (!enemy.IsAlive)
            {
                player.GainExperience(enemy.ExperienceReward);
                player.Gold += enemy.GoldReward;
                Console.WriteLine($"{player.Name} gains {enemy.ExperienceReward} experience and {enemy.GoldReward} gold!");
                
                Item loot = enemy.DropLoot();
                if (loot != null)
                {
                    player.Inventory.Add(loot);
                    Console.WriteLine($"{player.Name} found {loot.Name}!");
                }
                
                return CombatResult.Victory;
            }
            
            return CombatResult.Escape;
        }
    }
    
    public class GameUI
    {
        private Player player;
        private CombatSystem combatSystem = new CombatSystem();
        private bool running = true;
        
        public void Run()
        {
            Console.WriteLine("=== RPG Game ===");
            Console.WriteLine("Welcome to the adventure!");
            
            CreateCharacter();
            
            while (running)
            {
                ShowMainMenu();
            }
        }
        
        private void CreateCharacter()
        {
            Console.WriteLine("\n=== Character Creation ===");
            Console.Write("Enter your name: ");
            string name = Console.ReadLine();
            
            Console.WriteLine("\nChoose your class:");
            Console.WriteLine("1. Warrior - High health and defense");
            Console.WriteLine("2. Mage - Powerful magic attacks");
            Console.WriteLine("3. Rogue - Fast and stealthy");
            Console.Write("Choose class (1-3): ");
            
            string choice = Console.ReadLine();
            CharacterClass characterClass = choice switch
            {
                "1" => CharacterClass.Warrior,
                "2" => CharacterClass.Mage,
                "3" => CharacterClass.Rogue,
                _ => CharacterClass.Warrior
            };
            
            player = new Player(name, characterClass);
            GiveStartingItems();
            Console.WriteLine($"\nWelcome, {player.Name} the {characterClass}!");
            Console.WriteLine(player);
        }
        
        private void GiveStartingItems()
        {
            // Give starting weapon
            var startingWeapon = characterClass switch
            {
                CharacterClass.Warrior => new Weapon { Name = "Rusty Sword", Damage = 5, Value = 10, WeaponType = WeaponType.Sword },
                CharacterClass.Mage => new Weapon { Name = "Wooden Staff", Damage = 3, Value = 8, WeaponType = WeaponType.Staff },
                CharacterClass.Rogue => new Weapon { Name = "Dagger", Damage = 4, Value = 6, WeaponType = WeaponType.Dagger },
                _ => new Weapon { Name = "Fists", Damage = 1, Value = 0, WeaponType = WeaponType.Dagger }
            };
            
            player.Inventory.Add(startingWeapon);
            startingWeapon.Use(player);
            
            // Give starting potions
            var healthPotion = new Potion { Name = "Health Potion", HealAmount = 20, PotionType = PotionType.Health, Value = 15, Quantity = 3 };
            var manaPotion = new Potion { Name = "Mana Potion", HealAmount = 20, PotionType = PotionType.Mana, Value = 12, Quantity = 2 };
            
            player.Inventory.Add(healthPotion);
            player.Inventory.Add(manaPotion);
            
            player.Gold = 50;
        }
        
        private void ShowMainMenu()
        {
            Console.WriteLine("\n=== Main Menu ===");
            Console.WriteLine("1. Explore");
            Console.WriteLine("2. Rest");
            Console.WriteLine("3. Inventory");
            Console.WriteLine("4. Character Stats");
            Console.WriteLine("5. Shop");
            Console.WriteLine("6. Exit");
            Console.Write("Choose an option: ");
            
            string choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    Explore();
                    break;
                case "2":
                    player.Rest();
                    break;
                case "3":
                    ShowInventory();
                    break;
                case "4":
                    ShowCharacterStats();
                    break;
                case "5":
                    ShowShop();
                    break;
                case "6":
                    running = false;
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
        }
        
        private void Explore()
        {
            Console.WriteLine("\nYou venture into the wilderness...");
            
            Random random = new Random();
            int encounter = random.Next(1, 4);
            
            switch (encounter)
            {
                case 1:
                    FightGoblin();
                    break;
                case 2:
                    FindTreasure();
                    break;
                case 3:
                    Console.WriteLine("You find nothing of interest.");
                    break;
            }
        }
        
        private void FightGoblin()
        {
            var goblin = new Enemy("Goblin", EnemyType.Goblin, player.Level);
            goblin.LootTable.Add(new Weapon { Name = "Goblin Dagger", Damage = 6, Value = 20, WeaponType = WeaponType.Dagger });
            goblin.LootTable.Add(new Potion { Name = "Small Health Potion", HealAmount = 10, PotionType = PotionType.Health, Value = 8 });
            
            var result = combatSystem.StartCombat(player, goblin);
            
            if (result == CombatResult.Defeat)
            {
                Console.WriteLine("You have been defeated! Game Over.");
                running = false;
            }
        }
        
        private void FindTreasure()
        {
            Random random = new Random();
            int goldFound = random.Next(10, 50);
            player.Gold += goldFound;
            Console.WriteLine($"You found a treasure chest with {goldFound} gold!");
        }
        
        private void ShowInventory()
        {
            Console.WriteLine("\n=== Inventory ===");
            if (!player.Inventory.Any())
            {
                Console.WriteLine("Your inventory is empty.");
            }
            else
            {
                for (int i = 0; i < player.Inventory.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {player.Inventory[i]}");
                }
                
                Console.WriteLine($"\nGold: {player.Gold}");
                Console.WriteLine($"Equipped Weapon: {player.EquippedWeapon?.Name ?? "None"}");
            }
        }
        
        private void ShowCharacterStats()
        {
            Console.WriteLine("\n=== Character Stats ===");
            Console.WriteLine(player);
            Console.WriteLine($"Gold: {player.Gold}");
            Console.WriteLine($"Experience: {player.Experience}/{player.Level * 100}");
        }
        
        private void ShowShop()
        {
            Console.WriteLine("\n=== Shop ===");
            Console.WriteLine("Welcome to the shop!");
            Console.WriteLine("1. Buy Health Potion (15 gold)");
            Console.WriteLine("2. Buy Mana Potion (12 gold)");
            Console.WriteLine("3. Buy Iron Sword (50 gold)");
            Console.WriteLine("4. Leave shop");
            Console.Write("Choose option: ");
            
            string choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    BuyItem(new Potion { Name = "Health Potion", HealAmount = 20, PotionType = PotionType.Health, Value = 15 }, 15);
                    break;
                case "2":
                    BuyItem(new Potion { Name = "Mana Potion", HealAmount = 20, PotionType = PotionType.Mana, Value = 12 }, 12);
                    break;
                case "3":
                    BuyItem(new Weapon { Name = "Iron Sword", Damage = 10, Value = 50, WeaponType = WeaponType.Sword }, 50);
                    break;
            }
        }
        
        private void BuyItem(Item item, int cost)
        {
            if (player.Gold >= cost)
            {
                player.Gold -= cost;
                player.Inventory.Add(item);
                Console.WriteLine($"Purchased {item.Name} for {cost} gold!");
            }
            else
            {
                Console.WriteLine("Not enough gold!");
            }
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            var gameUI = new GameUI();
            gameUI.Run();
            
            Console.WriteLine("Thanks for playing!");
        }
    }
}
