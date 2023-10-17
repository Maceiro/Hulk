
 public static class Parser {
  

  public static SymbolSet Firsts ;
  public static SymbolSet Follow;
  public static Symbol epsilon;
  public static Table table;
  public static bool processing;
  public static Node Tree;

  public static Node Parsing( List<Token> tokens ) {
   
   if( !processing ) {
     Pre_Processing() ;
     processing= true ;
   }
   
   return ConstructTree( tokens );
   
  }

  public static void Pre_Processing() {

   Data.Create_Gramatik();
   Calculate_Firsts();
   Calculate_Follow();
   BuildTable();
   //Program.Aux_Table();
   
  }


  public static void Calculate_Firsts() {
  
  var firsts= new SymbolSet();
  epsilon= Data.Epsilon;
   foreach( var t in Data.gramatik.Terminals ) {
  
   var list= new List<Symbol>();
   list.Add( t);
   firsts.Add( t, list );
   }

   foreach( var n in Data.gramatik.No_Terminals ) {

   firsts.Add( n, new List<Symbol>() );
   }

   bool change;
   do {
    
    change= false;
   foreach( var p in Data.gramatik.Productions ) {
   
   Symbol left= p.Left ;
   var right= p.Right;

   if(p.IsEpsilon ) { 
    if( change) firsts[left].Add_Bool( epsilon );
    else change= firsts[left].Add_Bool( epsilon );
    continue;
   }

   bool all_epsilon= true;
   foreach( var s in right ) {
    
    if( change) firsts[left].Add_All( firsts[s] );
     else change= firsts[left].Add_All( firsts[s] );

    if( !firsts[s].Contains(epsilon) ) {
     all_epsilon= false;
     break;
    }

     if( all_epsilon) {
      if( change) firsts[left].Add_Bool( epsilon );
       else change= firsts[left].Add_Bool( epsilon );
     }

    
    }

    }

   }
  while( change );

  Firsts= firsts ;
  }


  public static void BuildTable() {

  var table_aux= new Table();

  foreach( var p in Data.gramatik.Productions ) 
   foreach( var t in Data.gramatik.Terminals ) {
    
   // if( t.Class.Length== 0) continue;
    if( !p.IsEpsilon && Calculate_Firsts_Sufix( p.Right, 0 ).Contains_Bool( t) ) table_aux.Add( p.Left, t, p );
    if( p.IsEpsilon && Follow[ p.Left].Contains_Bool( t) ) table_aux.Add( p.Left, t, p ); 
   
   }

   table= table_aux ;
  
  }


  public static Node ConstructTree( List<Token> tokens ) {

   var s= new Stack<Symbol_Node>();
   s.Push( new Symbol_Node( Data.gramatik.Initial, null ) ) ;
   Node root= null ;
   int cursor= 0;
   var nodes= new Stack<Node>() ;
   var cursors= new Stack<int>() ;

   while( s.Count>0 && cursor< tokens.Count ) {
   
    bool error= false ;
    Symbol_Node actual= s.Pop() ;
    //if(cant< 500 ) Console.WriteLine( actual.Symbol.Class );
    var node= new Node( actual.Symbol.Class, actual.Ref );
    if( actual.Ref==null ) root= node;
    else actual.Ref.Children.Add( node );
    
    Symbol symbol= actual.Symbol;
    if( symbol.IsTerminal && !symbol.IsEpsilon ) {

      if( symbol.Class!= tokens[cursor].Class ) {

        if( nodes.Count==0 ) {
        Operation_System.Print_in_Console( "Sintax Error!! : Token " + symbol.Class + " espected after " + "\"" + tokens[cursor-1].Chain + "\"" );
        return null; 
        }
        else error= true ;
      }
      node.Chain= tokens[cursor].Chain ;
      cursor++;
    }

    else {
     if( symbol.IsEpsilon ) continue;
     else {

     List<Production> list= table.Search( symbol, new Symbol( tokens[cursor].Class ));
     if( list== null )  {
      if( nodes.Count==0 ) {
      Operation_System.Print_in_Console( "Sintax Error!! : " + Transform( symbol.Class, s ) + " espected after " + "\"" + tokens[cursor-1].Chain + "\"" ); 
      return null;
      }
      else error= true ;
     }

      if( list!= null ) {
      
       if( list.Count>1 ) {
       nodes.Push( node ) ;
       cursors.Push( cursor ) ;
       if( symbol.Class!= "general") s.Push( new Symbol_Node( new Symbol("limit"), null ) );

       }
       if( symbol.Class=="limit" ) {
         nodes.Pop(); 
         cursors.Pop();

       }

        for( int i= list[0].Right.Count-1; i>=0; i-- ) 
      s.Push( new Symbol_Node( list[0].Right[i], node ));

      }
      
     }

    }
    
      if( error ) {  
       
       string limit= node.Symbol ;
  
      // if( cant < 500) Console.WriteLine( "error ") ;
       node= nodes.Pop() ;
       node.Children= new List<Node>() ;
       cursor= cursors.Pop() ;
       var aux_list= table.Search( new Symbol( node.Symbol), new Symbol( tokens[cursor].Class ) ) ;
      //if( cant< 500 ) Console.WriteLine( "introduciendo la produccion de {0} que empieza por {1}", node.Symbol, aux_list[1].Right[0].Class ) ;
       
       if( limit!="limit") 
       while( limit!="limit") limit= s.Pop().Symbol.Class ;

       for( int i= aux_list[1].Right.Count-1; i>=0; i-- ) 
        s.Push( new Symbol_Node( aux_list[1].Right[i], node )) ;

      }

   }
   //Program.Print( root, 0);
   if(root.Check_Errors() ) return null ;
    Console.WriteLine( "Arbol_Construido") ;
   if( cursor== (tokens.Count-1) )  return root;
    else return null;
  }


  
   public static void Calculate_Follow() {
   
   var follow= new SymbolSet();
   foreach( var n in Data.gramatik.No_Terminals ) 
    follow.Add( n, new List<Symbol>() ) ;
   
   follow[Data.gramatik.Initial].Add( Data.EOF );
   bool change;
   do {
    change= false; 
   foreach( var p in Data.gramatik.Productions ) {

    var left= p.Left ;
    var right= p.Right ;
    
    for( int i= 0; i< right.Count; i++ ) {
      
      if( right[i].IsTerminal ) continue;

      var firsts= Calculate_Firsts_Sufix( right, i+1 );

      if( change) follow[right[i]].Add_All( firsts);
       else change= follow[right[i]].Add_All( firsts);


      if( firsts.Contains_Epsilon() || i== (right.Count-1) ) {
        
        if( change) follow[ right[i] ].Add_All( follow[left]);
         else change= follow[ right[i] ].Add_All( follow[left]);
       
      }

    }

   }
      
   }
    while( change);

    Follow=  follow ;

   }


   public static List<Symbol> Calculate_Firsts_Sufix( List<Symbol> symbols, int ini ) {

    var result= new List<Symbol>() ;
    if( ini>= symbols.Count ) return result;
    bool all_epsilon= true;

    for( int i= ini; i< symbols.Count; i++ ) {

      result.Add_All( Firsts[ symbols[i] ] );
      if( !Firsts[symbols[i]].Contains(epsilon) ) {
        all_epsilon= false;
        break;
      }
    }

    if( all_epsilon ) result.Add( epsilon);

    return result;

   }

   public static string Transform( string s, Stack<Symbol_Node> stack) {

    if( s=="expr" || s=="term" || s=="factor" ) return "Expression" ;
    if( s=="atom" || s=="mol") return "ID, Function_Call, Number or String" ;
    if( s=="condition") return "Condition" ;
    if( s=="list_expr") return "Expression List";
    if( s=="list_arg") return "Argument List" ;
    if( s=="line") return "Expression or Statement";
    if( s=="statement") return "Statement";
    if( s=="list_assignments") return "Assignment List" ;
    if( s=="op") return "Comparison Operator";
    if( s=="aux_expr" || s=="aux_term" || s=="aux_factor" ) {
    var aux1= stack.Pop() ;
    string aux2=aux1.Symbol.Class ;
    return Transform( aux2, stack );
    }
    if(s=="aux_list_arg") return ") or Argument List";
    if(s=="aux_list_expr") return ") or Expression List";
    if(s=="aux_list_assignments") return "token \"in\" or more assignments" ;
    //Modificar estos 3 ultimos para que pidan al usuario agregar comas "
    
    return s;
  
   }


 } 

 public static class Node_Extensions {

  public static bool Check_Errors( this Node node ) {
   
   if( ( node.Symbol=="list_arg" || node.Symbol=="list_expr" ) && node.Children.Count==1 && node.Parent!=null && node.Parent.Children[0].Symbol==",") {
   Operation_System.Print_in_Console( "Syntax Error!! : Expression List or Argument List expected after \",\"") ;
   return true ;
   }
   for(int i= 0; i< node.Children.Count; i++ )
    if( node.Children[i].Check_Errors()) return true ;

    return false ;

  }


 }

