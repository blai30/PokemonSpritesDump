# PokemonSpritesDump

![image](https://github.com/user-attachments/assets/86b2ae1a-3a91-42fa-8aa9-17ba7f3e1714)

## Options
### `appsettings.json` / `appsettings.Development.json`
- ApiOptions
  - Limit: integer (default 1026: downloads all 1025 Pokemon including forms)
    - The ending index; exclusive.
  - Offset: integer (default 0: starts from 1)
    - The starting index; exclusive.
  - BruteForce: true, false (default false)
    - Attempt to request all possible filename combinations from the CDN (Note: this can possibly send up to 300,000+ requests to the CDN and will take a very long time to complete, not recommended to set true).

- ImageOptions
  - Format: png, jpeg, webp (default webp: converts original png to webp)
    - When png is specified, no conversion is performed as the original file is already png.
  - Quality: integer (default 100)
    - Only applicable to jpeg and webp formats.
  - Lossless: true, false (default true)
    - Only applicable to jpeg and webp formats.

## Important things to know
- This tool uses the [PokeAPI](https://pokeapi.co/) to capture `pokemon-species`, `pokemon`, and `pokemon-form` objects which are used to generate the filenames for the downloaded sprites.
  - This means that many web HTTP requests will be made, more than the specified limit as many Pokemon will have multiple forms.
  - Some Pokemon such as Pikachu, Arceus, Kyurem, Vivillon, Necrozma, and possibly more, will have inaccurate filenames as the ordering of `pokemon-form` from PokeAPI does not necessarily match the Pokemon HOME CDN.
    - Most should be accurate though including Unown, Gourgeist, Florges, Minior, Alcremie, most if not all mega evolutions, and possibly more.
- This tool performs file-based caching, located in `./out/cache`.
  - This cache will be accessed on subsequent executions of this tool to prevent suffocating the PokeAPI and the Pokemon HOME CDN.
  - Delete this cache to call the API from a fresh slate.
- Downloaded sprites are located in `./out/sprites`.
  - Existing files will be skipped on subsequent executions of this tool.
