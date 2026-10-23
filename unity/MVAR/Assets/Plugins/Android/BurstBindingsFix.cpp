#include <stdint.h>

// Weak symbol fallback for Unity 6 missing internal call symbol during IL2CPP linking
__attribute__((weak)) void Register_UnityEngine_Bindings_BindingsLoader_BurstGetBindingsFunctionsForModule() {
    // Fallback stub
}
