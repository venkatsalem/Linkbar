{$WARN SYMBOL_PLATFORM OFF}                                                     { Disable warning "Symbol '..' is specific to a platform" }

{$IFNDEF DEBUG}
  {$WEAKLINKRTTI ON}
  {$RTTI EXPLICIT METHODS([]) PROPERTIES([]) FIELDS([])}
{$ENDIF}

{;$DEFINE DEBUGUS}

{$IFDEF RELEASE}
  { Size Optimizations }
  {$OPTIMIZATION ON}                 { Enable code optimization }
  {$STRINGSPOOLING ON}               { Enable string pooling to reduce size }
  {$STRINGCHECKS OFF}                { Disable string range checking for speed }
  {$IOCHECKS OFF}                    { Disable I/O checking for speed }
  {$OVERFLOWCHECKS OFF}              { Disable overflow checking for speed }
  {$RANGECHECKS OFF}                { Disable range checking for speed }
  {$STACKFRAMES OFF}                 { Eliminate stack frames for faster code }
  {$LOCALSYMBOLS OFF}                { Remove local symbols for smaller size }
  {$DEFINITIONINFO OFF}              { Remove definition info for smaller size }
  {$REFLECTION OFF}                  { Disable reflection for smaller size }
  {$ASSERTIONS OFF}                  { Disable assertions for speed }
  
  { Performance Optimizations }
  {$ALIGN 8}                        { 8-byte alignment for better performance on 64-bit }
  {$MINENUMSIZE 1}                  { Use smallest possible enum size }
  
  { Advanced Optimizations }
  {$VARSTRINGCHECKS OFF}             { Disable variant string checks }
  {$OPENSTRINGS OFF}                 { Disable open string parameters }
  {$TYPEINFO OFF}                    { Disable RTTI type info }
  {$WRITEABLECONST OFF}               { Keep constants in read-only section }
  {$SAFEDIVIDE OFF}                 { Disable safe divide checks }
{$ENDIF}
