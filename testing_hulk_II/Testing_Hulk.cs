
 public class Program {

  public static void Main() {

  Operation_System.Interface() ;
  
  }

  public static void Aux() {

  foreach( var pair in Parser.Follow.dicc )  {
   Console.WriteLine( "Key" + "  " + pair.Key );
   foreach( var s in pair.Value )
    if( s.IsEpsilon ) Console.WriteLine( "epsilon" + "  ");
    else Console.Write( s.Class + "  " );

    Console.WriteLine( "\n");
  }

  }


   public static void Aux_Table() {

     foreach( var n in Data.gramatik.No_Terminals ) 
      foreach( var t in Data.gramatik.Terminals ) {
       Console.Write( "Si tenemos "+ n.Class + " y "+ t.Class + " entonces aplicamos :    ");
       var list= Parser.table.Search( n, t);
       if( list== null ) Console.Write( "null");
       else {
        foreach( var p in list ) {
       Console.Write( p.Left.Class + " => ");
       foreach( var s in p.Right )
       Console.Write( s.Class + " "); 
       Console.Write("\n") ;
        }    

       }

       Console.Write( "\n");

      }
   }


   public static void Print( Node node, int height ) {

    Console.WriteLine( node.Symbol + "   " + height );
    foreach( var tree in node.Children )
    Print( tree, height+1 );

   }

  

 }

