# Game Development Project in C#

A comprehensive game development project that demonstrates game programming concepts, object-oriented design, and interactive application development.

## Project Overview

This game development project will create a text-based adventure game with:
- Character creation and management
- Inventory system
- Combat mechanics
- World exploration
- Quest system
- Save/load functionality

## Features

### Core Game Features
- Character creation with classes and stats
- Turn-based combat system
- Inventory and item management
- Experience and leveling system
- World map with different locations
- NPC interactions

### Advanced Features
- Quest system with objectives
- Equipment and weapons
- Magic and special abilities
- Save/load game state
- Random encounters
- Story progression

### Technical Features
- Game state management
- Event-driven programming
- Random number generation
- Data persistence
- Modifiable game data
- Extensible architecture

## Project Structure

```
GameProject/
├── Models/
│   ├── Character.cs
│   ├── Player.cs
│   ├── Enemy.cs
│   ├── Item.cs
│   ├── Weapon.cs
│   ├── Quest.cs
│   └── Location.cs
├── Systems/
│   ├── ICombatSystem.cs
│   ├── CombatSystem.cs
│   ├── IInventorySystem.cs
│   ├── InventorySystem.cs
│   ├── IQuestSystem.cs
│   └── QuestSystem.cs
├── Services/
│   ├── IGameService.cs
│   ├── GameService.cs
│   ├── ISaveService.cs
│   └── JsonSaveService.cs
├── UI/
│   ├── GameUI.cs
│   └── MenuRenderer.cs
├── Data/
│   ├── Characters.json
│   ├── Items.json
│   ├── Quests.json
│   └── Locations.json
└── Program.cs
```

## Core Classes

### Character Base Class
```csharp
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
        return baseDamage + new Random().Next(-2, 3); // Add some randomness
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
        
        // Check for level up
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
```

### Player Class
```csharp
public class Player : Character
{
    public CharacterClass CharacterClass { get; set; }
    public int Mana { get; set; }
    public int MaxMana { get; set; }
    public List<Quest> ActiveQuests { get; set; } = new List<Quest>();
    public List<Quest> CompletedQuests { get; set; } = new List<Quest>();
    public Location CurrentLocation { get; set; }
    
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
        
        // Class-specific level up bonuses
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
```

### Enemy Class
```csharp
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
        if (LootTable.Any() && new Random().NextDouble() < 0.5) // 50% chance to drop loot
        {
            return LootTable[new Random().Next(LootTable.Count)];
        }
        return null;
    }
}
```

### Item System
```csharp
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
```

### Combat System
```csharp
public interface ICombatSystem
{
    CombatResult StartCombat(Player player, Enemy enemy);
    void PlayerAttack(Player player, Enemy enemy);
    void EnemyAttack(Enemy enemy, Player player);
    bool UseSkill(Player player, Enemy enemy, string skill);
}

public class CombatSystem : ICombatSystem
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
                UseSkill(player, enemy, "");
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
        System.Threading.Thread.Sleep(1000); // Add delay for dramatic effect
        int damage = enemy.CalculateDamage();
        player.TakeDamage(damage);
        Console.WriteLine($"{enemy.Name} attacks {player.Name} for {damage} damage!");
    }
    
    public bool UseSkill(Player player, Enemy enemy, string skill)
    {
        switch (player.CharacterClass)
        {
            case CharacterClass.Warrior:
                return UseWarriorSkill(player, enemy, skill);
            case CharacterClass.Mage:
                return UseMageSkill(player, enemy, skill);
            case CharacterClass.Rogue:
                return UseRogueSkill(player, enemy, skill);
        }
        return false;
    }
    
    private bool UseWarriorSkill(Player player, Enemy enemy, string skill)
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
    
    private bool UseMageSkill(Player player, Enemy enemy, string skill)
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
    
    private bool UseRogueSkill(Player player, Enemy enemy, string skill)
    {
        Console.WriteLine("Rogue Skills:");
        Console.WriteLine("1. Backstab (6 MP) - Triple damage");
        Console.WriteLine("2. Evade (4 MP) - Dodge next attack");
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
        // Higher speed = higher chance to run
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

public enum CombatResult
{
    Victory,
    Defeat,
    Escape
}
```

## Implementation Steps

### Step 1: Create Core Models
1. Define Character, Player, and Enemy classes
2. Create Item hierarchy
3. Implement stats and progression system

### Step 2: Build Game Systems
1. Implement combat system
2. Create inventory management
3. Build quest system

### Step 3: Create Game World
1. Design locations and maps
2. Create NPCs and dialogue
3. Implement quest system

### Step 4: Add User Interface
1. Create game UI
2. Implement menu system
3. Add save/load functionality

## Usage Examples

### Starting a New Game
```csharp
var player = new Player("Hero", CharacterClass.Warrior);
var gameService = new GameService();
gameService.StartNewGame(player);
```

### Combat Example
```csharp
var enemy = new Enemy("Goblin", EnemyType.Goblin, 1);
var combatSystem = new CombatSystem();
var result = combatSystem.StartCombat(player, enemy);

if (result == CombatResult.Victory)
{
    Console.WriteLine("Victory!");
}
```

## Game Data Structure

### JSON Configuration
```json
{
  "enemies": [
    {
      "name": "Goblin",
      "type": "Goblin",
      "baseHealth": 30,
      "baseAttack": 5,
      "baseDefense": 2,
      "experienceReward": 20,
      "goldReward": 10
    }
  ],
  "items": [
    {
      "id": 1,
      "name": "Sword",
      "type": "Weapon",
      "damage": 10,
      "value": 50
    }
  ]
}
```

## Extension Ideas

1. **Graphical Interface**: Create a 2D game using Unity or MonoGame
2. **Multiplayer**: Add network multiplayer support
3. **Procedural Generation**: Generate random dungeons and worlds
4. **AI System**: Implement more sophisticated enemy AI
5. **Crafting System**: Add item crafting and recipes
6. **Faction System**: Add reputation and faction relationships
7. **Sound System**: Add audio effects and music
8. **Achievements**: Add achievement system

## Learning Objectives

This project helps you learn:
- Game programming concepts
- State management
- Event-driven programming
- Random number generation
- Data serialization
- Object-oriented design
- Game loop implementation
- User interface design
- Algorithm design for game mechanics

## Best Practices Demonstrated

- Separation of concerns
- Component-based architecture
- Data-driven design
- State pattern implementation
- Strategy pattern for combat
- Factory pattern for entities
- Observer pattern for events
- Singleton pattern for game manager
- Template method pattern for characters
- Command pattern for actions
