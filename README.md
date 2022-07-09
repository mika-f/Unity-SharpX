# SharpX Plugin for Unity

SharpX Plugin(s) for Unity.

## Requirements

- CsprojHooks for generating `.csproj` for SharpX in Unity.

## Packages

- SharpX-Core
- SharpX-ShaderLab
  - SharpX-ShaderLab-Boolean
  - SharpX-ShaderLab-Enum
- SharpX-Hlsl
  - SharpX-Hlsl-Enum
  - SharpX-Hlsl-ObjectInitializer

## Example `sharpx.config.json`

```json
{
  "compilerOptions": {
    "target": "ShaderLab",
    "lib": [],
    // relative path to output directory
    "outDir": "../Shaders",
    // relative path to base directory
    "baseUrl": "../Sources~/"
  },
  "files": [],
  // relative path to including files
  "includes": ["../Sources~/", "../Sources/", "../Editor/"],
  "excludes": [],
  // relative path to language implementations
  "languages": [
    "../../SharpX/Plugins/SharpX.ShaderLab.dll",
    "../../SharpX/Plugins/SharpX.Hlsl.dll"
  ],
  // relative path to language backend plugins
  "plugins": [
    "../../SharpX/Plugins/SharpX.ShaderLab.CSharp.dll",
    "../../SharpX/Plugins/SharpX.ShaderLab.CSharp.Boolean.dll",
    "../../SharpX/Plugins/SharpX.ShaderLab.CSharp.Enum.dll",
    "../../SharpX/Plugins/SharpX.Hlsl.CSharp.dll",
    "../../SharpX/Plugins/SharpX.Hlsl.CSharp.Enum.dll",
    "../../SharpX/Plugins/SharpX.Hlsl.CSharp.ObjectInitializer.dll"
  ]
}
```

If you want to use C# for writing Shaders in `Assets/YourCompany/Shader/Sources~` directory, put `sharpx.config.json` into `Sources~` directory and configure settings likes below:

- SharpX Custom Compilation
  - `YourCompany/Shader/Sources~`
- SharpX External Reference Assemblies
  - `NatsunekoLaboratory/SharpX/Plugins/SharpX.Hlsl.Primitives.dll`
  - `NatsunekoLaboratory/SharpX/Plugins/SharpX.ShaderLab.Primitives.dll`
  - `and other your primitive libraries...`
- SharpX External Reference Projects
  - `if you want to share source code between editor, configure it`
- Assembly Location
  - None

## License

MIT by [@6jz](https://twitter.com/6jz)
