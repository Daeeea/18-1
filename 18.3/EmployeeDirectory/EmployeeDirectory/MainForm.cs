using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace EmployeeDirectory
{
    public class MainForm : Form
    {
        // Данные
        private List<Employee> employees;
        private int nextId = 1;
        private string saveFile = "employees.json";
        private Employee currentEmployee;

        // Контролы
        private DataGridView grid;
        private TextBox txtSearch;
        private ComboBox cmbDepartment;
        private TextBox txtLastName, txtFirstName, txtMiddleName;
        private DateTimePicker dtpBirth;
        private ComboBox cmbGender;
        private TextBox txtPhone, txtEmail;
        private NumericUpDown numSalary;
        private CheckBox chkActive;
        private Label lblStatus;
        private Button btnAdd, btnEdit, btnDelete, btnSave, btnExport, btnPrint;

        public MainForm()
        {
            this.Text = "Справочник сотрудников";
            this.Size = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            LoadData();
            CreateUI();
            RefreshGrid();
        }

        // Модель сотрудника
        private class Employee
        {
            public int Id { get; set; }
            public string LastName { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public DateTime BirthDate { get; set; }
            public string Gender { get; set; }
            public string Department { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public decimal Salary { get; set; }
            public bool IsActive { get; set; }

            public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
            public int Age => DateTime.Today.Year - BirthDate.Year;
        }

        private void CreateUI()
        {
            // Верхняя панель поиска
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.FromArgb(52, 73, 94), Padding = new Padding(5) };

            Label lblSearch = new Label { Text = "Поиск:", Location = new Point(10, 10), Size = new Size(50, 25), ForeColor = Color.White };
            txtSearch = new TextBox { Location = new Point(60, 8), Size = new Size(200, 25) };
            txtSearch.TextChanged += (s, e) => RefreshGrid();

            Label lblDept = new Label { Text = "Отдел:", Location = new Point(280, 10), Size = new Size(50, 25), ForeColor = Color.White };
            cmbDepartment = new ComboBox { Location = new Point(330, 8), Size = new Size(150, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDepartment.Items.AddRange(new string[] { "Все", "ИТ", "Бухгалтерия", "HR", "Продажи", "Маркетинг" });
            cmbDepartment.SelectedIndex = 0;
            cmbDepartment.SelectedIndexChanged += (s, e) => RefreshGrid();

            Button btnClear = new Button { Text = "Сброс", Location = new Point(500, 6), Size = new Size(70, 30), BackColor = Color.Gray, ForeColor = Color.White };
            btnClear.Click += (s, e) => { txtSearch.Text = ""; cmbDepartment.SelectedIndex = 0; };

            topPanel.Controls.AddRange(new Control[] { lblSearch, txtSearch, lblDept, cmbDepartment, btnClear });

            // Таблица сотрудников
            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                BackgroundColor = Color.White
            };
            grid.SelectionChanged += Grid_SelectionChanged;

            // Панель деталей
            Panel detailPanel = new Panel { Dock = DockStyle.Bottom, Height = 220, BackColor = Color.FromArgb(240, 248, 255), BorderStyle = BorderStyle.FixedSingle };

            int x = 10, y = 10, w = 180;

            // ФИО
            Label lblFIO = new Label { Text = "Фамилия:", Location = new Point(x, y), Size = new Size(70, 25) };
            txtLastName = new TextBox { Location = new Point(x + 75, y), Size = new Size(w, 25) };

            Label lblName = new Label { Text = "Имя:", Location = new Point(x + 75 + w + 10, y), Size = new Size(50, 25) };
            txtFirstName = new TextBox { Location = new Point(x + 75 + w + 65, y), Size = new Size(w, 25) };
            y += 35;

            Label lblMiddle = new Label { Text = "Отчество:", Location = new Point(x, y), Size = new Size(70, 25) };
            txtMiddleName = new TextBox { Location = new Point(x + 75, y), Size = new Size(w, 25) };

            Label lblBirth = new Label { Text = "Дата рожд.:", Location = new Point(x + 75 + w + 10, y), Size = new Size(70, 25) };
            dtpBirth = new DateTimePicker { Location = new Point(x + 75 + w + 85, y), Size = new Size(w, 25), Format = DateTimePickerFormat.Short };
            dtpBirth.Value = new DateTime(1990, 1, 1);
            y += 35;

            Label lblGender = new Label { Text = "Пол:", Location = new Point(x, y), Size = new Size(70, 25) };
            cmbGender = new ComboBox { Location = new Point(x + 75, y), Size = new Size(100, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbGender.Items.AddRange(new string[] { "Мужской", "Женский" });
            cmbGender.SelectedIndex = 0;

            Label lblPhone = new Label { Text = "Телефон:", Location = new Point(x + 75 + w + 10, y), Size = new Size(70, 25) };
            txtPhone = new TextBox { Location = new Point(x + 75 + w + 85, y), Size = new Size(w, 25) };
            y += 35;

            Label lblEmail = new Label { Text = "Email:", Location = new Point(x, y), Size = new Size(70, 25) };
            txtEmail = new TextBox { Location = new Point(x + 75, y), Size = new Size(w, 25) };

            Label lblSalary = new Label { Text = "Оклад:", Location = new Point(x + 75 + w + 10, y), Size = new Size(70, 25) };
            numSalary = new NumericUpDown { Location = new Point(x + 75 + w + 85, y), Size = new Size(w, 25), Minimum = 0, Maximum = 1000000, ThousandsSeparator = true };
            y += 35;

            chkActive = new CheckBox { Text = "Активен", Location = new Point(x, y), Size = new Size(100, 25) };

            Button btnSaveDetail = new Button { Text = "Сохранить", Location = new Point(x + 150, y), Size = new Size(100, 30), BackColor = Color.Green, ForeColor = Color.White };
            btnSaveDetail.Click += BtnSaveDetail_Click;

            detailPanel.Controls.AddRange(new Control[] {
                lblFIO, txtLastName, lblName, txtFirstName, lblMiddle, txtMiddleName,
                lblBirth, dtpBirth, lblGender, cmbGender, lblPhone, txtPhone,
                lblEmail, txtEmail, lblSalary, numSalary, chkActive, btnSaveDetail
            });

            // Нижняя панель кнопок
            Panel bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.FromArgb(52, 73, 94) };

            btnAdd = new Button { Text = "Добавить", Location = new Point(10, 8), Size = new Size(100, 35), BackColor = Color.Green, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnEdit = new Button { Text = "Редактировать", Location = new Point(120, 8), Size = new Size(100, 35), BackColor = Color.Blue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete = new Button { Text = "Удалить", Location = new Point(230, 8), Size = new Size(100, 35), BackColor = Color.Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSave = new Button { Text = "Сохранить JSON", Location = new Point(340, 8), Size = new Size(100, 35), BackColor = Color.Teal, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnExport = new Button { Text = "Экспорт CSV", Location = new Point(450, 8), Size = new Size(100, 35), BackColor = Color.Purple, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnPrint = new Button { Text = "Печать", Location = new Point(560, 8), Size = new Size(100, 35), BackColor = Color.Orange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnSave.Click += BtnSave_Click;
            btnExport.Click += BtnExport_Click;
            btnPrint.Click += BtnPrint_Click;

            lblStatus = new Label { Text = "Готов", Location = new Point(700, 15), Size = new Size(300, 25), ForeColor = Color.White };

            bottomPanel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnSave, btnExport, btnPrint, lblStatus });

            // Сборка
            this.Controls.Add(grid);
            this.Controls.Add(detailPanel);
            this.Controls.Add(topPanel);
            this.Controls.Add(bottomPanel);
        }

        private void LoadData()
        {
            try
            {
                if (File.Exists(saveFile))
                {
                    string json = File.ReadAllText(saveFile);
                    employees = JsonConvert.DeserializeObject<List<Employee>>(json);
                    if (employees == null) employees = new List<Employee>();
                    if (employees.Count > 0) nextId = employees.Max(e => e.Id) + 1;
                    else nextId = 1;
                }
                else
                {
                    employees = new List<Employee>();
                    CreateSampleData();
                }
            }
            catch
            {
                employees = new List<Employee>();
                CreateSampleData();
            }
        }

        private void CreateSampleData()
        {
            employees.Add(new Employee
            {
                Id = nextId++,
                LastName = "Иванов",
                FirstName = "Иван",
                MiddleName = "Иванович",
                BirthDate = new DateTime(1990, 5, 15),
                Gender = "Мужской",
                Department = "ИТ",
                Phone = "+7 (999) 123-45-67",
                Email = "ivanov@mail.ru",
                Salary = 85000,
                IsActive = true
            });

            employees.Add(new Employee
            {
                Id = nextId++,
                LastName = "Петрова",
                FirstName = "Мария",
                MiddleName = "Сергеевна",
                BirthDate = new DateTime(1992, 8, 23),
                Gender = "Женский",
                Department = "Бухгалтерия",
                Phone = "+7 (999) 234-56-78",
                Email = "petrova@mail.ru",
                Salary = 75000,
                IsActive = true
            });

            employees.Add(new Employee
            {
                Id = nextId++,
                LastName = "Сидоров",
                FirstName = "Петр",
                MiddleName = "Алексеевич",
                BirthDate = new DateTime(1988, 12, 10),
                Gender = "Мужской",
                Department = "HR",
                Phone = "+7 (999) 345-67-89",
                Email = "sidorov@mail.ru",
                Salary = 65000,
                IsActive = true
            });
        }

        private void SaveToFile()
        {
            try
            {
                string json = JsonConvert.SerializeObject(employees, Formatting.Indented);
                File.WriteAllText(saveFile, json);
                lblStatus.Text = $"Сохранено {employees.Count} сотрудников";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }

        private void RefreshGrid()
        {
            grid.Columns.Clear();
            grid.Columns.Add("Id", "ID");
            grid.Columns.Add("FullName", "ФИО");
            grid.Columns.Add("Department", "Отдел");
            grid.Columns.Add("Phone", "Телефон");
            grid.Columns.Add("Salary", "Оклад");
            grid.Columns.Add("Status", "Статус");

            grid.Columns["Id"].Width = 50;
            grid.Columns["FullName"].Width = 200;
            grid.Columns["Department"].Width = 100;
            grid.Columns["Phone"].Width = 120;
            grid.Columns["Salary"].Width = 100;

            var list = employees.AsEnumerable();

            string search = txtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(search))
                list = list.Where(e => e.FullName.ToLower().Contains(search) || e.Phone.Contains(search));

            if (cmbDepartment.SelectedIndex > 0)
                list = list.Where(e => e.Department == cmbDepartment.SelectedItem.ToString());

            grid.Rows.Clear();
            foreach (var e in list)
            {
                grid.Rows.Add(e.Id, e.FullName, e.Department, e.Phone, e.Salary.ToString("C2"), e.IsActive ? "Активен" : "Неактивен");
            }

            lblStatus.Text = $"Всего: {employees.Count} | Показано: {list.Count()}";
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count > 0)
            {
                int id = (int)grid.SelectedRows[0].Cells["Id"].Value;
                currentEmployee = employees.FirstOrDefault(emp => emp.Id == id);
                if (currentEmployee != null)
                {
                    txtLastName.Text = currentEmployee.LastName;
                    txtFirstName.Text = currentEmployee.FirstName;
                    txtMiddleName.Text = currentEmployee.MiddleName;
                    dtpBirth.Value = currentEmployee.BirthDate;
                    cmbGender.SelectedItem = currentEmployee.Gender;
                    txtPhone.Text = currentEmployee.Phone;
                    txtEmail.Text = currentEmployee.Email;
                    numSalary.Value = currentEmployee.Salary;
                    chkActive.Checked = currentEmployee.IsActive;
                }
            }
        }

        private void ClearForm()
        {
            txtLastName.Text = "";
            txtFirstName.Text = "";
            txtMiddleName.Text = "";
            dtpBirth.Value = new DateTime(1990, 1, 1);
            cmbGender.SelectedIndex = 0;
            txtPhone.Text = "";
            txtEmail.Text = "";
            numSalary.Value = 0;
            chkActive.Checked = true;
        }

        private void BtnSaveDetail_Click(object sender, EventArgs e)
        {
            if (currentEmployee == null)
            {
                MessageBox.Show("Выберите сотрудника");
                return;
            }

            currentEmployee.LastName = txtLastName.Text;
            currentEmployee.FirstName = txtFirstName.Text;
            currentEmployee.MiddleName = txtMiddleName.Text;
            currentEmployee.BirthDate = dtpBirth.Value;
            currentEmployee.Gender = cmbGender.SelectedItem?.ToString() ?? "Мужской";
            currentEmployee.Phone = txtPhone.Text;
            currentEmployee.Email = txtEmail.Text;
            currentEmployee.Salary = numSalary.Value;
            currentEmployee.IsActive = chkActive.Checked;

            RefreshGrid();
            SaveToFile();
            lblStatus.Text = "Данные сохранены";
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            Employee newEmp = new Employee
            {
                Id = nextId++,
                LastName = "Новый",
                FirstName = "Сотрудник",
                MiddleName = "",
                BirthDate = new DateTime(1990, 1, 1),
                Gender = "Мужской",
                Department = cmbDepartment.SelectedIndex > 0 ? cmbDepartment.SelectedItem.ToString() : "ИТ",
                Phone = "",
                Email = "",
                Salary = 30000,
                IsActive = true
            };

            employees.Add(newEmp);
            RefreshGrid();
            SaveToFile();
            lblStatus.Text = $"Добавлен: {newEmp.FullName}";
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (currentEmployee != null)
            {
                txtLastName.Focus();
                lblStatus.Text = "Измените данные и нажмите 'Сохранить'";
            }
            else
            {
                MessageBox.Show("Выберите сотрудника");
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (currentEmployee != null)
            {
                if (MessageBox.Show($"Удалить {currentEmployee.FullName}?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    employees.Remove(currentEmployee);
                    currentEmployee = null;
                    ClearForm();
                    RefreshGrid();
                    SaveToFile();
                    lblStatus.Text = "Сотрудник удален";
                }
            }
            else
            {
                MessageBox.Show("Выберите сотрудника");
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveToFile();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "CSV файлы|*.csv";
            dlg.FileName = $"employees_{DateTime.Now:yyyyMMdd}.csv";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(dlg.FileName, false, Encoding.UTF8))
                    {
                        sw.WriteLine("ID,ФИО,Отдел,Телефон,Email,Оклад,Статус");
                        foreach (var emp in employees)
                        {
                            sw.WriteLine($"{emp.Id},{emp.FullName},{emp.Department},{emp.Phone},{emp.Email},{emp.Salary},{(emp.IsActive ? "Активен" : "Неактивен")}");
                        }
                    }
                    MessageBox.Show($"Экспорт завершен!", "Успех");
                    lblStatus.Text = $"Экспортировано {employees.Count} записей";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (currentEmployee == null)
            {
                MessageBox.Show("Выберите сотрудника");
                return;
            }

            PrintDocument pd = new PrintDocument();
            pd.PrintPage += (s, ev) =>
            {
                Font titleFont = new Font("Arial", 16, FontStyle.Bold);
                Font textFont = new Font("Arial", 10);
                Brush brush = Brushes.Black;
                int y = 50;

                ev.Graphics.DrawString("КАРТОЧКА СОТРУДНИКА", titleFont, brush, 150, y);
                y += 50;
                ev.Graphics.DrawLine(Pens.Black, 50, y, 550, y);
                y += 30;

                ev.Graphics.DrawString($"ФИО: {currentEmployee.FullName}", textFont, brush, 50, y); y += 25;
                ev.Graphics.DrawString($"Дата рождения: {currentEmployee.BirthDate:dd.MM.yyyy}", textFont, brush, 50, y); y += 25;
                ev.Graphics.DrawString($"Возраст: {currentEmployee.Age}", textFont, brush, 50, y); y += 25;
                ev.Graphics.DrawString($"Пол: {currentEmployee.Gender}", textFont, brush, 50, y); y += 25;
                ev.Graphics.DrawString($"Отдел: {currentEmployee.Department}", textFont, brush, 50, y); y += 25;
                ev.Graphics.DrawString($"Телефон: {currentEmployee.Phone}", textFont, brush, 50, y); y += 25;
                ev.Graphics.DrawString($"Email: {currentEmployee.Email}", textFont, brush, 50, y); y += 25;
                ev.Graphics.DrawString($"Оклад: {currentEmployee.Salary:C2}", textFont, brush, 50, y); y += 25;
                ev.Graphics.DrawString($"Статус: {(currentEmployee.IsActive ? "Активен" : "Неактивен")}", textFont, brush, 50, y);
            };

            PrintPreviewDialog preview = new PrintPreviewDialog();
            preview.Document = pd;
            preview.ShowDialog();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveToFile();
            base.OnFormClosing(e);
        }
    }
}