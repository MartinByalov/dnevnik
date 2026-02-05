// ООП И БАЗИ ДАННИ
// --------------------------------------------------------------------------------
// 1. КЛАСОВЕ И ОБЕКТИ: Класът е шаблон (Person), а обектите са конкретните инстанции (Student, Teacher).
// 2. ЕНКАПСУЛАЦИЯ: Използване на свойства (get; set;) за контролиран достъп до данните.
// 3. НАСЛЕДЯВАНЕ (Inheritance): Подкласовете поемат характеристиките на базовия клас (Person -> Student).
// 4. ПОЛИМОРФИЗМ (Polymorphism): Пренаписване на методи (override), така че един метод да действа различно.
// 5. АБСТРАКЦИЯ: Скриване на сложните SQL детайли зад прости бутони и методи в интерфейса.
// 6. ПЪРВИЧЕН КЛЮЧ (Primary Key): Уникален идентификатор за всеки запис в базата (ID).
// --------------------------------------------------------------------------------

// Program.cs
// dotnet add package System.Data.OleDb
// accessdatabaseengine_X64
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO;

// Базов клас "Човек"
public class Person
{
    // Свойства (Properties) за съхранение на данни
    public int ID { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";

    // Конструктор: Метод за инициализиране на обекта при създаването му с 'new'
    public Person(int id, string firstName, string lastName)
    {
        ID = id;
        FirstName = firstName;
        LastName = lastName;
    }

    // Виртуален метод: Позволява на наследниците да го променят (полиморфизъм)
    public virtual string GetInfo() => $"{FirstName} {LastName}";

    // Пренаписване на системния метод ToString за нуждите на ListBox
    public override string ToString() => GetInfo();
}

// Наследник "Учител"
public class Teacher : Person
{
    public string Subject { get; set; } = "";

    // Извикване на конструктора на базовия клас чрез 'base'
    public Teacher(int id, string firstName, string lastName, string subject) 
        : base(id, firstName, lastName) => Subject = subject;

    // Полиморфизъм: Промяна на поведението на GetInfo за учители
    public override string GetInfo() => $"[УЧИТЕЛ] {base.GetInfo()} - {Subject}";
}

// Наследник "Ученик"
public class Student : Person
{
    public string ClassName { get; set; } = "";

    public Student(int id, string firstName, string lastName, string className) 
        : base(id, firstName, lastName) => ClassName = className;

    // Полиморфизъм: Промяна на поведението на GetInfo за ученици
    public override string GetInfo() => $"[УЧЕНИК] {base.GetInfo()} ({ClassName})";
}

// Главен клас за интерфейса, наследяващ системния клас Form
public class ProgramForm : Form
{
    // Компоненти на потребителския интерфейс (Controls)
    private ListBox lstData = new ListBox();
    private Button btnLoad = new Button();
    private Button btnDelete = new Button();
    private Button btnAdd = new Button();
    
    private TextBox txtFirstName = new TextBox();
    private TextBox txtLastName = new TextBox();
    private TextBox txtClass = new TextBox();
    private Label lblStatus = new Label();

    // Път до базата и Connection String за връзка с Access
    private string dbPath = Path.Combine(Application.StartupPath, "dnevnik.accdb");
    private string ConnString => $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};";

    // Конструктор на формата: Настройка на визуалните елементи
    public ProgramForm()
    {
        this.Text = "Училищен Дневник - ООП и Бази Данни";
        this.Size = new System.Drawing.Size(800, 550);

        // Позициониране на елементите върху екрана
        lstData.Bounds = new System.Drawing.Rectangle(20, 20, 450, 350);
        
        btnLoad.Bounds = new System.Drawing.Rectangle(20, 380, 210, 45);
        btnLoad.Text = "ЗАРЕДИ ДАННИ";
        // Абониране за събитие (Event Handling) чрез ламбда израз
        btnLoad.Click += (s, e) => RefreshAll();

        btnDelete.Bounds = new System.Drawing.Rectangle(260, 380, 210, 45);
        btnDelete.Text = "ИЗТРИЙ ИЗБРАНИЯ";
        btnDelete.Click += (s, e) => DeleteSelected();

        txtFirstName.Bounds = new System.Drawing.Rectangle(500, 50, 250, 30);
        txtFirstName.PlaceholderText = "Име на ученик";
        
        txtLastName.Bounds = new System.Drawing.Rectangle(500, 90, 250, 30);
        txtLastName.PlaceholderText = "Фамилия на ученик";
        
        txtClass.Bounds = new System.Drawing.Rectangle(500, 130, 250, 30);
        txtClass.PlaceholderText = "Клас (напр. 6А)";

        btnAdd.Bounds = new System.Drawing.Rectangle(500, 180, 250, 45);
        btnAdd.Text = "ДОБАВИ УЧЕНИК";
        btnAdd.Click += (s, e) => AddStudent();

        lblStatus.Bounds = new System.Drawing.Rectangle(20, 450, 750, 25);
        lblStatus.Text = "База данни: " + dbPath;

        // Добавяне на контролите към формата
        this.Controls.AddRange(new Control[] { lstData, btnLoad, btnDelete, txtFirstName, txtLastName, txtClass, btnAdd, lblStatus });
    }

    // Метод за извличане на данни (Четене от база данни)
    private void RefreshAll()
    {
        if (!File.Exists(dbPath)) { MessageBox.Show("Базата не е намерена в директорията на програмата!"); return; }
        
        lstData.Items.Clear();
        using var conn = new OleDbConnection(ConnString);
        try
        {
            conn.Open(); // Отваряне на връзката
            
            // SQL заявка за учители
            var cmdT = new OleDbCommand("SELECT teacher_id, first_name, last_name, subject FROM teachers", conn);
            using var rT = cmdT.ExecuteReader();
            while (rT.Read()) 
                lstData.Items.Add(new Teacher((int)rT[0], rT[1].ToString(), rT[2].ToString(), rT[3].ToString()));

            // SQL заявка за ученици
            var cmdS = new OleDbCommand("SELECT student_id, first_name, last_name, class FROM students", conn);
            using var rS = cmdS.ExecuteReader();
            while (rS.Read()) 
                lstData.Items.Add(new Student((int)rS[0], rS[1].ToString(), rS[2].ToString(), rS[3].ToString()));
        }
        catch (Exception ex) { MessageBox.Show("Грешка при четене: " + ex.Message); }
    }

    // Метод за записване (Добавяне в база данни)
    private void AddStudent()
    {
        if (string.IsNullOrWhiteSpace(txtFirstName.Text)) return;

        using var conn = new OleDbConnection(ConnString);
        try
        {
            conn.Open();

            // Ръчно генериране на нов Първичен Ключ (Primary Key)
            int nextId = 1;
            var cmdId = new OleDbCommand("SELECT MAX(student_id) FROM students", conn);
            var result = cmdId.ExecuteScalar();
            if (result != DBNull.Value && result != null) nextId = Convert.ToInt32(result) + 1;

            // Използване на параметризирана SQL заявка за сигурност
            string sql = "INSERT INTO students (student_id, first_name, last_name, class, egn) VALUES (?, ?, ?, ?, ?)";
            using var cmd = new OleDbCommand(sql, conn);
            cmd.Parameters.AddWithValue("?", nextId);
            cmd.Parameters.AddWithValue("?", txtFirstName.Text);
            cmd.Parameters.AddWithValue("?", txtLastName.Text);
            cmd.Parameters.AddWithValue("?", txtClass.Text);
            cmd.Parameters.AddWithValue("?", DateTime.Now.Ticks.ToString().Substring(0, 10));
            
            cmd.ExecuteNonQuery(); // Изпълнение на записа
            RefreshAll(); // Опресняване на списъка
            txtFirstName.Clear(); txtLastName.Clear(); txtClass.Clear();
        }
        catch (Exception ex) { MessageBox.Show("Грешка при запис: " + ex.Message); }
    }

    // Метод за премахване (Изтриване от база данни)
    private void DeleteSelected()
    {
        // Проверка дали е избран обект и кастване към базовия тип Person
        if (lstData.SelectedItem is not Person p) return;

        // Динамично определяне на таблицата според типа на обекта
        string table = (p is Teacher) ? "teachers" : "students";
        string idCol = (p is Teacher) ? "teacher_id" : "student_id";

        using var conn = new OleDbConnection(ConnString);
        try
        {
            conn.Open();
            using var cmd = new OleDbCommand($"DELETE FROM {table} WHERE {idCol} = ?", conn);
            cmd.Parameters.AddWithValue("?", p.ID);
            cmd.ExecuteNonQuery();
            RefreshAll();
        }
        catch (Exception ex) { MessageBox.Show("Грешка при изтриване: " + ex.Message); }
    }

    // Входна точка на приложението
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new ProgramForm());
    }
}