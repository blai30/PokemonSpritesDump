namespace PokemonSpritesDump;

public static class MapSorter
{
    public static Dictionary<int, List<string>> PokemonSlugs =>
        new()
        {
            {
                25,
                [
                    "pikachu",
                    "pikachu-original-cap",
                    "pikachu-hoenn-cap",
                    "pikachu-sinnoh-cap",
                    "pikachu-unova-cap",
                    "pikachu-kalos-cap",
                    "pikachu-alola-cap",
                    "pikachu-partner-cap",
                    "pikachu-starter",
                    "pikachu-world-cap",
                ]
            },
            {
                493,
                [
                    "arceus-normal",
                    // "arceus",
                    "arceus-fighting",
                    "arceus-flying",
                    "arceus-poison",
                    "arceus-ground",
                    "arceus-rock",
                    "arceus-bug",
                    "arceus-ghost",
                    "arceus-steel",
                    "arceus-fire",
                    "arceus-water",
                    "arceus-grass",
                    "arceus-electric",
                    "arceus-psychic",
                    "arceus-ice",
                    "arceus-dragon",
                    "arceus-dark",
                    "arceus-fairy",
                ]
            },
            {
                666,
                [
                    "vivillon-icy-snow",
                    "vivillon-polar",
                    "vivillon-tundra",
                    "vivillon-continental",
                    "vivillon-garden",
                    "vivillon-elegant",
                    "vivillon-meadow",
                    // "vivillon",
                    "vivillon-modern",
                    "vivillon-marine",
                    "vivillon-archipelago",
                    "vivillon-high-plains",
                    "vivillon-sandstorm",
                    "vivillon-river",
                    "vivillon-monsoon",
                    "vivillon-savanna",
                    "vivillon-sun",
                    "vivillon-ocean",
                    "vivillon-jungle",
                    "vivillon-fancy",
                    "vivillon-poke-ball",
                ]
            },
            {
                718,
                [
                    "zygarde-50",
                    "zygarde-10",
                    "zygarde-50-power-construct",
                    "zygarde-10-power-construct",
                    "zygarde-complete",
                ]
            },
            {
                925,
                [
                    "maushold-family-of-three",
                    "maushold-family-of-four",
                ]
            },
            {
                1007,
                [
                    "koraidon-apex-build",
                    "koraidon-sprinting-build",
                    "koraidon-limited-build",
                    "koraidon-swimming-build",
                    "koraidon-gliding-build",
                ]
            },
            {
                1008,
                [
                    "miraidon-ultimate-mode",
                    "miraidon-drive-mode",
                    "miraidon-low-power-mode",
                    "miraidon-aquatic-mode",
                    "miraidon-glide-mode",
                ]
            },
        };

    public static List<string> ItemSlugs =>
    [
        "", // 0
        "master-ball", // 1
        "ultra-ball", // 2
        "great-ball", // 3
        "poke-ball", // 4
        "safari-ball", // 5
        "net-ball", // 6
        "dive-ball", // 7
        "nest-ball", // 8
        "repeat-ball", // 9
        "timer-ball", // 10
        "luxury-ball", // 11
        "premier-ball", // 12
        "dusk-ball", // 13
        "heal-ball", // 14
        "quick-ball", // 15
        "cherish-ball", // 16
        "potion", // 17
        "antidote", // 18
        "burn-heal", // 19
        "ice-heal", // 20
        "awakening", // 21
        "paralyze-heal", // 22
        "full-restore", // 23
        "max-potion", // 24
        "hyper-potion", // 25
        "super-potion", // 26
        "full-heal", // 27
        "revive", // 28
        "max-revive", // 29
        "fresh-water", // 30
        "soda-pop", // 31
        "lemonade", // 32
        "moomoo-milk", // 33
        "energy-powder", // 34
        "energy-root", // 35
        "heal-powder", // 36
        "revival-herb", // 37
        "ether", // 38
        "max-ether", // 39
        "elixir", // 40
        "max-elixir", // 41
        "", // 42
        "", // 43
        "", // 44
        "hp-up", // 45
        "protein", // 46
        "iron", // 47
        "carbos", // 48
        "calcium", // 49
        "rare-candy", // 50
        "pp-up", // 51
        "zinc", // 52
        "pp-max", // 53
        "", // 54
        "guard-spec", // 55
        "dire-hit", // 56
        "x-attack", // 57
        "x-defense", // 58
        "x-speed", // 59
        "x-accuracy", // 60
        "x-sp-attack", // 61
        "x-sp-defense", // 62
        "poke-doll", // 63
        "", // 64
        "", // 65
        "", // 66
        "", // 67
        "", // 68
        "", // 69
        "", // 70
        "", // 71
        "", // 72
        "", // 73
        "", // 74
        "", // 75
        "super-repel", // 76
        "max-repel", // 77
        "escape-rope", // 78
        "repel", // 79
        "sun-stone", // 80
        "moon-stone", // 81
        "fire-stone", // 82
        "thunder-stone", // 83
        "water-stone", // 84
        "leaf-stone", // 85
        "tiny-mushroom", // 86
        "big-mushroom", // 87
        "pearl", // 88
        "big-pearl", // 89
        "stardust", // 90
        "star-piece", // 91
        "nugget", // 92
        "", // 93
        "honey", // 94
        "", // 95
        "", // 96
        "", // 97
        "", // 98
        "", // 99
        "", // 100
        "", // 101
        "", // 102
        "", // 103
        "", // 104
        "", // 105
        "rare-bone", // 106
        "shiny-stone", // 107
        "dusk-stone", // 108
        "dawn-stone", // 109
        "oval-stone", // 110
        "odd-keystone", // 111
        "griseous-orb", // 112
        "", // 113
        "", // 114
        "", // 115
        "", // 116
        "", // 117
        "", // 118
        "", // 119
        "", // 120
        "", // 121
        "", // 122
        "", // 123
        "", // 124
        "", // 125
        "", // 126
        "", // 127
        "", // 128
        "", // 129
        "", // 130
        "", // 131
        "", // 132
        "", // 133
        "", // 134
        "adamant-orb", // 135
        "lustrous-orb", // 136
    ];
}
