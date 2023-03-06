using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFD.Objects;
using SFDGameScriptInterface;
using SFR.Helper;

namespace SFR.Game
{
    internal sealed class ExtendedModifiers
    {
        // Keeps track of all created ExtendedModifiers.
        internal static readonly List<ExtendedModifiers> ExtendedModifiersList = new();

        // The unique PlayerModifiers linked to this instance.
        private readonly PlayerModifiers _playerModifiers;

        // Custom modifiers.
        public float JumpHeightModifier;

        /// <summary>
        /// Creates a new instance of <see cref="ExtendedModifiers"/> and links a <see cref="PlayerModifiers"/> to it.
        /// </summary>
        /// <param name="playerModifiers">The <see cref="PlayerModifiers"/> instance to link with this.</param>
        internal ExtendedModifiers(PlayerModifiers playerModifiers) => _playerModifiers = playerModifiers;

        // Custom code.



        // ====
    }
}
