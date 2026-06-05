using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace TaskManagerApp
{
    public class MainForm : Form
    {
        // Поля формы
        private DataGridView grid;
        private TextBox txtTitle;
        private TextBox txtDesc;
        private DateTimePicker dtpDate;
        private ComboBox cmbPriority;
        private CheckBox chkDone;
        private Label lblStatus;

        // Данные
        private List<TaskItem> tasks;
        private int nextId = 1;
        private string saveFile = "tasks.json";

        public MainForm()
        {
            // Настройка окна
            this.Text = "Мои задачи";
            this.Size = new Size(900, 550);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Загрузка данных
            LoadData();

            // Создание интерфейса
            CreateInterface();

            // Обновление таблицы
            RefreshGrid();
        }

        // Модель задачи
        private class TaskItem
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime DueDate { get; set; }
            public int Priority { get; set; } // 1=Низкий, 2=Средний, 3=Высокий
            public bool IsDone { get; set; }
        }

        private void CreateInterface()
        {
            // Панель ввода
            Panel inputPanel = new Panel();
            inputPanel.Dock = DockStyle.Top;
            inputPanel.Height = 150;
            inputPanel.BackColor = Color.FromArgb(52, 73, 94);
            inputPanel.Padding = new Padding(10);

            // Название
            Label lblTitle = new Label() { Text = "Название:", Location = new Point(10, 15), Size = new Size(70, 25), ForeColor = Color.White };
            txtTitle = new TextBox() { Location = new Point(85, 12), Size = new Size(300, 25) };

            // Описание
            Label lblDesc = new Label() { Text = "Описание:", Location = new Point(10, 50), Size = new Size(70, 25), ForeColor = Color.White };
            txtDesc = new TextBox() { Location = new Point(85, 47), Size = new Size(300, 50), Multiline = true };

            // Дата
            Label lblDate = new Label() { Text = "Срок:", Location = new Point(10, 110), Size = new Size(70, 25), ForeColor = Color.White };
            dtpDate = new DateTimePicker() { Location = new Point(85, 107), Size = new Size(130, 25), Format = DateTimePickerFormat.Short };
            dtpDate.Value = DateTime.Now.AddDays(7);

            // Приоритет
            Label lblPriority = new Label() { Text = "Приоритет:", Location = new Point(230, 110), Size = new Size(70, 25), ForeColor = Color.White };
            cmbPriority = new ComboBox() { Location = new Point(305, 107), Size = new Size(100, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPriority.Items.AddRange(new object[] { "Низкий", "Средний", "Высокий" });
            cmbPriority.SelectedIndex = 1;

            // Статус
            chkDone = new CheckBox() { Text = "Выполнена", Location = new Point(420, 110), Size = new Size(90, 25), ForeColor = Color.White };

            // Кнопки
            Button btnAdd = new Button() { Text = "Добавить", Location = new Point(530, 15), Size = new Size(100, 35), BackColor = Color.Green, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAdd.Click += BtnAdd_Click;

            Button btnEdit = new Button() { Text = "Изменить", Location = new Point(530, 60), Size = new Size(100, 35), BackColor = Color.Blue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnEdit.Click += BtnEdit_Click;

            Button btnDelete = new Button() { Text = "Удалить", Location = new Point(530, 105), Size = new Size(100, 35), BackColor = Color.Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete.Click += BtnDelete_Click;

            inputPanel.Controls.AddRange(new Control[] { lblTitle, txtTitle, lblDesc, txtDesc, lblDate, dtpDate, lblPriority, cmbPriority, chkDone, btnAdd, btnEdit, btnDelete });

            // Таблица задач
            grid = new DataGridView();
            grid.Dock = DockStyle.Fill;
            grid.AllowUserToAddRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.ReadOnly = true;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.RowHeadersVisible = false;
            grid.BackgroundColor = Color.White;
            grid.SelectionChanged += Grid_SelectionChanged;

            // Нижняя панель
            Panel bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 40;
            bottomPanel.BackColor = Color.FromArgb(52, 73, 94);

            Button btnSave = new Button() { Text = "Сохранить", Location = new Point(10, 5), Size = new Size(100, 30), BackColor = Color.Blue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.Click += BtnSave_Click;

            Button btnExport = new Button() { Text = "Экспорт CSV", Location = new Point(120, 5), Size = new Size(100, 30), BackColor = Color.Green, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnExport.Click += BtnExport_Click;

            lblStatus = new Label() { Text = "Готов", Location = new Point(240, 10), Size = new Size(300, 25), ForeColor = Color.White };

            bottomPanel.Controls.AddRange(new Control[] { btnSave, btnExport, lblStatus });

            // Добавляем все на форму
            this.Controls.Add(grid);
            this.Controls.Add(inputPanel);
            this.Controls.Add(bottomPanel);
        }

        private void LoadData()
        {
            try
            {
                if (File.Exists(saveFile))
                {
                    string json = File.ReadAllText(saveFile);
                    tasks = JsonConvert.DeserializeObject<List<TaskItem>>(json);

                    if (tasks == null) tasks = new List<TaskItem>();
                    if (tasks.Count > 0) nextId = tasks.Max(t => t.Id) + 1;
                }
                else
                {
                    tasks = new List<TaskItem>();
                    CreateSampleData();
                }
            }
            catch
            {
                tasks = new List<TaskItem>();
                CreateSampleData();
            }
        }

        private void CreateSampleData()
        {
            tasks.Add(new TaskItem { Id = nextId++, Title = "Изучить C#", Description = "Прочитать книгу", DueDate = DateTime.Now.AddDays(7), Priority = 3, IsDone = false });
            tasks.Add(new TaskItem { Id = nextId++, Title = "Сделать проект", Description = "Написать приложение", DueDate = DateTime.Now.AddDays(3), Priority = 2, IsDone = false });
            tasks.Add(new TaskItem { Id = nextId++, Title = "Сдать работу", Description = "Показать результат", DueDate = DateTime.Now.AddDays(1), Priority = 1, IsDone = false });
        }

        private void RefreshGrid()
        {
            // Настройка колонок
            grid.Columns.Clear();
            grid.Columns.Add("Id", "ID");
            grid.Columns.Add("Title", "Название");
            grid.Columns.Add("Priority", "Приоритет");
            grid.Columns.Add("DueDate", "Срок");
            grid.Columns.Add("Status", "Статус");
            grid.Columns.Add("Description", "Описание");

            grid.Columns["Id"].Width = 50;
            grid.Columns["Title"].Width = 150;
            grid.Columns["Priority"].Width = 80;
            grid.Columns["DueDate"].Width = 100;
            grid.Columns["Status"].Width = 80;

            // Заполнение данными
            grid.Rows.Clear();
            foreach (var task in tasks)
            {
                string priorityText = task.Priority == 3 ? "Высокий" : (task.Priority == 2 ? "Средний" : "Низкий");
                string statusText = task.IsDone ? "Выполнена" : "В работе";
                string dueDate = task.DueDate.ToString("dd.MM.yyyy");

                grid.Rows.Add(task.Id, task.Title, priorityText, dueDate, statusText, task.Description);
            }

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            int total = tasks.Count;
            int done = tasks.Count(t => t.IsDone);
            lblStatus.Text = $"Всего: {total} | Выполнено: {done} | Осталось: {total - done}";
        }

        private void SaveData()
        {
            try
            {
                string json = JsonConvert.SerializeObject(tasks, Formatting.Indented);
                File.WriteAllText(saveFile, json);
                lblStatus.Text = $"Сохранено {tasks.Count} задач";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }

        private void ExportToCsv()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "CSV файлы|*.csv";
            dlg.FileName = $"tasks_{DateTime.Now:yyyyMMdd}.csv";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(dlg.FileName, false, Encoding.UTF8))
                    {
                        sw.WriteLine("ID,Название,Описание,Приоритет,Срок,Статус");

                        foreach (var t in tasks)
                        {
                            string priority = t.Priority == 3 ? "Высокий" : (t.Priority == 2 ? "Средний" : "Низкий");
                            string status = t.IsDone ? "Выполнена" : "В работе";
                            string title = t.Title.Contains(",") ? $"\"{t.Title}\"" : t.Title;
                            string desc = t.Description?.Contains(",") == true ? $"\"{t.Description}\"" : (t.Description ?? "");

                            sw.WriteLine($"{t.Id},{title},{desc},{priority},{t.DueDate:dd.MM.yyyy},{status}");
                        }
                    }

                    MessageBox.Show("Экспорт завершен!", "Успех");
                    lblStatus.Text = $"Экспортировано {tasks.Count} задач";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка экспорта: " + ex.Message);
                }
            }
        }

        private void ClearForm()
        {
            txtTitle.Text = "";
            txtDesc.Text = "";
            dtpDate.Value = DateTime.Now.AddDays(7);
            cmbPriority.SelectedIndex = 1;
            chkDone.Checked = false;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Введите название задачи!");
                return;
            }

            TaskItem newTask = new TaskItem()
            {
                Id = nextId++,
                Title = txtTitle.Text.Trim(),
                Description = txtDesc.Text.Trim(),
                DueDate = dtpDate.Value,
                Priority = cmbPriority.SelectedIndex + 1,
                IsDone = chkDone.Checked
            };

            tasks.Add(newTask);
            RefreshGrid();
            ClearForm();
            SaveData();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите задачу для редактирования!");
                return;
            }

            int id = (int)grid.SelectedRows[0].Cells["Id"].Value;
            var task = tasks.FirstOrDefault(t => t.Id == id);

            if (task != null)
            {
                task.Title = txtTitle.Text.Trim();
                task.Description = txtDesc.Text.Trim();
                task.DueDate = dtpDate.Value;
                task.Priority = cmbPriority.SelectedIndex + 1;
                task.IsDone = chkDone.Checked;

                RefreshGrid();
                ClearForm();
                SaveData();

                MessageBox.Show("Задача обновлена!");
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите задачу для удаления!");
                return;
            }

            int id = (int)grid.SelectedRows[0].Cells["Id"].Value;
            var task = tasks.FirstOrDefault(t => t.Id == id);

            if (task != null)
            {
                if (MessageBox.Show($"Удалить задачу '{task.Title}'?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    tasks.Remove(task);
                    RefreshGrid();
                    ClearForm();
                    SaveData();
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveData();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            ExportToCsv();
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count > 0)
            {
                int id = (int)grid.SelectedRows[0].Cells["Id"].Value;
                var task = tasks.FirstOrDefault(t => t.Id == id);

                if (task != null)
                {
                    txtTitle.Text = task.Title;
                    txtDesc.Text = task.Description;
                    dtpDate.Value = task.DueDate;
                    cmbPriority.SelectedIndex = task.Priority - 1;
                    chkDone.Checked = task.IsDone;
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveData();
            base.OnFormClosing(e);
        }
    }
}