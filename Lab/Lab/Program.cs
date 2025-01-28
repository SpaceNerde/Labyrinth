namespace Lab;

// TODO:
// [x] Generate map for player to walk in
// [X] Generate simple labyrinth
// [] Create Rooms and Doors
// [X] Create Goal to Reach and end

// ROOM
// [x] Room has id and name
// [] Every room has doors
// [X] One room is the goal
// [X] Can contain key
// [] Doors can be locked

// PLAYER
// [X] Allow player to move
// [X] Can collect Keys

// MODES
// [] Easy mode -> No timer, No Keys
// [] Normal mode -> Spawn keys and locked doors, No timer
// [] Hard mode -> Add timer, most doors are locked

// Map
// . -> Normal Ground
// # -> Wall
// o -> Player
// k -> Key
// d -> Door
// G -> Goal

// I'm suprised this mess of a game actually is working....

// Ehh doors are boring just pick up a key and then walk to the goal :D

public enum TileType
{
    Floor, 
    Wall,
    PlayerTile,
    Key,
    Goal,
}

public enum GameState
{
    Running,
    Ended,
}

class Program
{
    static void Main(string[] args)
    {
        Map map = new Map(80, 20, 4, 6, 20);
        
        (int player_x, int player_y) = map.rooms[0].Center();
        Player player = new Player(player_x, player_y);

        GameContext gameContext = new GameContext(map, player);
        
        // Game Loop
        while (true)
        {
            if (gameContext.gameState == GameState.Ended) break;
            
            gameContext.Tick();
        }
        
        // Game Won
        Console.Clear();
        Console.WriteLine("YOU WON!!!");
    }
}

// simple rooms
class Room
{
    public int x1;
    public int y1;
    public int x2;
    public int y2;

    private String name;
    private String id;

    public Room(int x, int y, int w, int h)
    {
        this.x1 = x;
        this.y1 = y;
        this.x2 = x +w;
        this.y2 = y +h;
    }

    public bool Intersects(Room other)
    {
        // It is a true beauty
        return this.x1 <= other.x2 && this.x2 >= other.x1 && this.y1 <= other.y2 && this.y2 >= other.y1;
    }

    public (int, int) Center()
    {
        return ((this.x1 + this.x2) / 2, (this.y1 + this.y2) / 2);
    }
}

class Player
{
    public int x;
    public int y;

    public bool hasKey;

    public Player(int x, int y)
    {
        this.x = x;
        this.y = y;
        
        hasKey = false;
    }

    public void Move(int x, int y, Map map)
    {
        // pls dont move throug walls, thats mean!
        if (map.mapTiles[map.xy_idx(this.x + x, this. y + y)] == TileType.Wall) return;
        
        this.x += x;
        this.y += y;
    }
}

// I really am sorry for having you read throug this mess 
class Map
{
    public int mapHeight; 
    public int mapWidth;
    
    public int maxRooms;
    public int minSize;
    public int maxSize; 
    
    public TileType[] mapTiles;
    public List<Room> rooms;

    public Map(int mapWidth, int mapHeight, int minSize, int maxSize, int maxRooms)
    {
        this.mapHeight = mapHeight;
        this.mapWidth = mapWidth;
            
        this.maxRooms = maxRooms;
        this.maxSize = maxSize;
        this.minSize = minSize;
        
        this.mapTiles = new TileType[mapWidth * mapHeight];
        this.rooms = new List<Room>();
        
        this.NewMap();
    }
   
    // Util function to convert 2d to 1d
    public int xy_idx(int x, int y)
    {
        return (y * mapWidth) + x;
    }

    private void ApplyRoomToMap( Room room)
    {
        for (int y = room.y1 + 1; y <= room.y2; y++)
        {
            for (int x = room.x1 + 1; x <= room.x2; x++)
            {
                mapTiles[xy_idx(x, y)] = TileType.Floor;
            }
        }
    }
   
    // Creates Horizontal Tunnels between Rooms
    private void ApplyHTunnel(int x1, int x2, int y)
    {
        int start = Math.Min(x1, x2);
        int end = Math.Max(x1, x2);
        
        for (int x = start; x <= end; x++)
        {
            int idx = xy_idx(x, y);
            if (idx is > 0 && idx < mapWidth * mapHeight)
            {
                mapTiles[idx] = TileType.Floor;
            }
        }
    }
   
    // Creates Vertical Tunnels between Rooms
    private void ApplyVTunnel(int y1, int y2, int x)
    {
        int start = Math.Min(y1, y2);
        int end = Math.Max(y1, y2);

        for (int y = start; y <= end; y++)
        {
            int idx = xy_idx(x, y);
            if (idx is > 0 && idx < mapWidth * mapHeight)
            {
                mapTiles[idx] = TileType.Floor;
            }
        }
    }

    public void NewMap()
    {
        // Create map made out of FloorType
        Array.Fill(mapTiles, TileType.Wall);
                        
        // Create Multiple Connect rooms... after 2 hours :,)
        var rand  = new Random();
                
        for (int i = 0; i < maxRooms; i++)
        {
            int width = rand.Next(minSize, maxSize + 1);
            int height = rand.Next(minSize, maxSize + 1);
            int x = rand.Next(1, mapWidth - width - 1) - 1;
            int y = rand.Next(1, mapHeight - height - 1) - 1;
                            
            Room room = new Room(x, y, width, height);
                            
            // check if rooms are intersecting
            bool ok = true;
                            
            foreach (var otherRoom in rooms)
            {
                if (room.Intersects(otherRoom))
                {
                    ok = false;
                }
                                
            }
                
            if (!ok) continue;
            ApplyRoomToMap(room);
                
            if (rooms.Count != 0)
            {
                (int new_x, int new_y) = room.Center();
                (int pre_x, int pre_y) = rooms[rooms.Count - 1].Center();
                
                if (rand.Next(0, 2) == 1)
                {
                    ApplyHTunnel(pre_x, new_x, pre_y);
                    ApplyVTunnel(pre_y, new_y, new_x);
                }
                else
                {
                    ApplyHTunnel(pre_x, new_x, new_y);
                    ApplyVTunnel(pre_y, new_y, pre_x);
                }
            }
                                
            rooms.Add(room);
        }
        // Add Key and Goal 
        (int kepos_x, int kepos_y) = rooms[rand.Next(1, rooms.Count)].Center();
        mapTiles[xy_idx(kepos_x, kepos_y)] = TileType.Key;

        // yeah its dirty i know!!!
        while (true)
        {
            (int goalpos_x, int goalpos_y) = rooms[rand.Next(1, rooms.Count)].Center();
            if (goalpos_x == kepos_x && goalpos_y == kepos_y) continue;
            mapTiles[xy_idx(goalpos_x, goalpos_y)] = TileType.Goal;
            
            break;
        }
    }
}

// Drawing the game
// Game Context
// Game Context is made out of Player Pos and Map
// Render the map and then render the player over it, makes stuff alot more ez... i hope
class GameContext
{
    public Map map;
    public Player player;
    
    private TileType[] gameTiles;

    public GameState gameState;

    public GameContext(Map map, Player player)
    {
        this.map = map;
        this.player = player;
        
        gameTiles = map.mapTiles;
        
        gameState = GameState.Running;
    }

    public void Tick()
    {
        Console.Clear();
        
        int x = 0;
        int y = 0;
        
        gameTiles = (TileType[]) map.mapTiles.Clone();
        gameTiles[map.xy_idx(player.x, player.y)] = TileType.PlayerTile;
        
        foreach (var tile in gameTiles)
        {
            switch (tile)
            {
                case TileType.Floor:
                    Console.Write(".");
                    break;
                case TileType.Wall:
                    Console.Write("#");
                    break;
                case TileType.PlayerTile:
                    Console.Write("o");
                    break;
                case TileType.Key:
                    Console.Write("K");
                    break;
                case TileType.Goal:
                    Console.Write("G");
                    break;
            }
            
            x++;
            if (x >= map.mapWidth)
            {
                Console.WriteLine();
                x = 0;
                y++;
            }
        }
        
        ConsoleKey pressed = Console.ReadKey().Key;
        switch (pressed)
        {
            case ConsoleKey.W:
                player.Move(0, -1, map);
                break;
            case ConsoleKey.S:
                player.Move(0, 1, map);
                break;
            case ConsoleKey.A:
                player.Move(-1, 0, map);
                break;
            case ConsoleKey.D:
                player.Move(1, 0, map);
                break;
        }
        
        // Check if player is above key
        if (map.mapTiles[map.xy_idx(player.x, player.y)] == TileType.Key)
        {
            map.mapTiles[map.xy_idx(player.x, player.y)] = TileType.Floor;
            player.hasKey = true;
        }
        
        // Check if player is on goal and has key
        if (player.hasKey && map.mapTiles[map.xy_idx(player.x, player.y)] == TileType.Goal)
        {
            gameState = GameState.Ended;
        }
    }
}