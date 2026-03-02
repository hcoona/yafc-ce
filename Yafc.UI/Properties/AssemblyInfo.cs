using System.Runtime.CompilerServices;

// Yafc.Parser legitimately references Yafc.UI (to create SDL textures) and must write
// to SystemIcons internal setters during the data-loading phase.
[assembly: InternalsVisibleTo("Yafc.Parser")]
