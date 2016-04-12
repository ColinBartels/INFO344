<html>
<head>
<title>Online PHP Script Execution</title>
</head>
<body>
<?php
   echo "<h1>Hello, PHP!</h1>";
   $myBool = TRUE;
   echo "\n";
   echo $myBool;
   $myInt = 10;
   echo "\n";
   echo $myInt;
   $myWord = "hello world";
   echo "\n";
   echo $myWord;
   $myArray = array("ice cream", "steak", "apples");
   echo "\n";
   echo $myArray[0];
   $myKeyValuePair = array("Dad" => "Joe", "Mom" => "Amy", "Bro" => "Jason");
   echo "\n";
   echo $myKeyValuePair['Dad'];

   for ($i = 0; $i < count($myArray); $i++){
       echo "\n";
       echo $myArray[$i];
   }

   $temp = "10" + 1;
   echo "\n";
   echo $temp;

   $temp = array(1,2,3);
   echo "\n";
   echo $temp[1];

   if (1 == "1") {
       echo "\n";
       echo "One";
   }

   if (1 === "1") {
       echo "\n";
       echo "Two";
   }

   echo "\n";
   echo "hello" . "world";

?>
</body>
</html>
