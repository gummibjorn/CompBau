using System;

namespace RappiSharp.IL {
  [Serializable]
  public enum OpCode {
    ldc_b,  // load constant bool (operand bool)        // ... -> ..., bool
    ldc_c,  // load constant char (operand char)        // ... -> ..., char
    ldc_i,  // load constant int (operand int)          // ... -> ..., int
    ldc_s,  // load constant string (operand string)    // ... -> ..., string
    ldnull, // load null                                // ... -> ..., null

    add,    // add                                      // ..., value1, value2 -> ..., result
    sub,    // subtract                                 // ..., value1, value2 -> ..., result (value1 - value2)
    mul,    // multiply                                 // ..., value1, value2 -> ..., result
    div,    // divide                                   // ..., value1, value2 -> ..., result (value1 / value2)
    mod,    // modulo                                   // ..., value1, value2 -> ..., result (value1 % value2)
    neg,    // negate int or bool                       // ..., value -> ..., result

    ceq,    // compare equal                            // ..., value1, value2 -> ..., result (bool)
    cne,    // compare unequal                          // ..., value1, value2 -> ..., result (bool)
    clt,    // compare less than                        // ..., value1, value2 -> ..., result (value1 < value2)
    cle,    // compare less equal                       // ..., value1, value2 -> ..., result (value1 <= value2)
    cgt,    // compare greater than                     // ..., value1, value2 -> ..., result (value1 > value2)
    cge,    // compare greater equal                    // ..., value1, value2 -> ..., result (value1 >= value2)

    brtrue, // conditional branch if true (operand relative instruction offset)  // ..., value -> ...
    brfalse, // conditional branch if false (operand relative instruction offset)  // ..., value -> ...
    br,     // unconditional branch (operand relative instruction offset)        // ... -> ...

    isinst, // type test (operand class ref)            // ..., objref -> ..., result (bool)
    castclass,  // type cast (operand class ref)        // ..., objref -> ... (if successful)
    
    ldarg,  // load argument (operand arg index)        // ... -> ..., value
    starg,  // store argument (operand arg index)       // ..., value -> ...
    ldloc,  // load local var (operand loc index)       // ... -> ..., value
    stloc,  // store local var (operand loc index)      // ..., value -> ...

    ldfld,  // load field (operand fld index)           // ..., objref -> ..., value
    stfld,  // store field (operand fld index)          // ..., objref, value -> ...

    ldelem,  // load array element                      // ..., arrayref, index -> ..., value
    stelem,  // store array element                     // ..., arrayref, index, value -> ...

    ldthis, // load this                                // ... -> ..., thisref
    newobj, // new object (operand class ref)           // ... -> ..., objref
    ldlen,  // load length                              // ..., arrayref -> ..., length
    newarr, // new array (operand array type ref)       // ..., length -> ..., arrayref

    call,   // call inbuilt method (operand method ref) // ..., arg1, .., argn -> ..., result (if any)
    callvirt, // virtual call (operand method ref)      // ..., objref, arg1, .., argn -> ..., result (if any)
    ret     // return                                   // retval (if any) -> (empty)
  }
}
