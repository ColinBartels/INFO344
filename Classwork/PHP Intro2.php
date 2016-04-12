$i = 0;
   while ($i <= 10) {
       echo $i++;
   }

   for ($i = 0; $i <= 10; $i++) {
       echo $i;
   }

   $array1 = array('k1' => 'v1', 'k2' => 'v2');
   foreach ($array1 as $k =>$v) {
       echo 'key=' .$k. 'value' .$v.';';
   }

   class Student {
       public $name = "Chris Paul";
       public function SubmitHomework() {
           echo "I'll submit it tomorrow...";
       }

       public function GetName() {
           return "Name is ".$this->name;
       }

       public function getAllStudents() {
           return array(new Student(), new Student(), new Student());
       }
   }

   $myStudent = new Student();
   $myStudent->SubmitHomework();

   echo $myStudent->GetName();
   echo $myStudent->name;
   echo "\n";
   $students = $myStudent->getAllStudents();
   foreach($students as $s) {
       echo $s->GetName();
   }
