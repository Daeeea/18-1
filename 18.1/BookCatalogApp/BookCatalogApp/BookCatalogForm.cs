using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace BookCatalogApp
{
    public class BookCatalogForm : Form 
    {
        private DataGridView dgvBooks;
        private TextBox txtFilter;
        private Button btnFilter, btnClear, btnAdd, btnDelete, btnSortByName, btnSortByPrice;
        private Label lblCount;
        private DataTable booksTable;
        private BindingSource bindingSource;
        private int nextId = 1;

        public BookCatalogForm()
        {
            this.Text = "Справочник книг";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            CreateData();
            CreateUI();
            LoadSampleData();
        }

        private void CreateData()
        {
            booksTable = new DataTable();
            booksTable.Columns.Add("ID", typeof(int));
            booksTable.Columns.Add("Название", typeof(string));
            booksTable.Columns.Add("Автор", typeof(string));
            booksTable.Columns.Add("Год", typeof(int));
            booksTable.Columns.Add("Цена", typeof(decimal));

            bindingSource = new BindingSource();
            bindingSource.DataSource = booksTable;
        }

        private void CreateUI()
        {
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(10) };

            Label lblFilter = new Label { Text = "Фильтр:", Location = new Point(10, 15), Size = new Size(50, 25) };
            txtFilter = new TextBox { Location = new Point(60, 12), Size = new Size(200, 25) };

            btnFilter = new Button { Text = "Найти", Location = new Point(270, 10), Size = new Size(80, 30), BackColor = Color.LightBlue };
            btnFilter.Click += (s, e) => FilterData();

            btnClear = new Button { Text = "Сброс", Location = new Point(360, 10), Size = new Size(80, 30), BackColor = Color.LightGray };
            btnClear.Click += (s, e) => ClearFilter();

            btnSortByName = new Button { Text = "По названию", Location = new Point(460, 10), Size = new Size(100, 30), BackColor = Color.LightGreen };
            btnSortByName.Click += (s, e) => bindingSource.Sort = "Название ASC";

            btnSortByPrice = new Button { Text = "По цене", Location = new Point(570, 10), Size = new Size(100, 30), BackColor = Color.LightGreen };
            btnSortByPrice.Click += (s, e) =>
            {
                if (bindingSource.Sort == "Цена ASC")
                    bindingSource.Sort = "Цена DESC";
                else
                    bindingSource.Sort = "Цена ASC";
            };

            btnAdd = new Button { Text = "Добавить", Location = new Point(10, 45), Size = new Size(100, 30), BackColor = Color.LightGreen };
            btnAdd.Click += (s, e) => AddBook();

            btnDelete = new Button { Text = "Удалить", Location = new Point(120, 45), Size = new Size(100, 30), BackColor = Color.LightCoral };
            btnDelete.Click += (s, e) => DeleteBook();

            lblCount = new Label { Text = "Записей: 0", Location = new Point(680, 50), Size = new Size(100, 25), TextAlign = ContentAlignment.MiddleRight };

            topPanel.Controls.AddRange(new Control[] { lblFilter, txtFilter, btnFilter, btnClear, btnSortByName, btnSortByPrice, btnAdd, btnDelete, lblCount });

            dgvBooks = new DataGridView { Dock = DockStyle.Fill, DataSource = bindingSource, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };

            this.Controls.Add(dgvBooks);
            this.Controls.Add(topPanel);
        }

        private void LoadSampleData()
        {
            booksTable.Rows.Add(nextId++, "Война и мир", "Лев Толстой", 1869, 850);
            booksTable.Rows.Add(nextId++, "Преступление и наказание", "Достоевский", 1866, 650);
            booksTable.Rows.Add(nextId++, "Мастер и Маргарита", "Булгаков", 1967, 750);
            booksTable.Rows.Add(nextId++, "Анна Каренина", "Лев Толстой", 1877, 700);
            booksTable.Rows.Add(nextId++, "Евгений Онегин", "Пушкин", 1833, 450);
            booksTable.Rows.Add(nextId++, "Тихий Дон", "Шолохов", 1940, 550);
            booksTable.Rows.Add(nextId++, "1984", "Оруэлл", 1949, 500);
            UpdateCount();
        }

        private void FilterData()
        {
            string filter = txtFilter.Text.Trim();
            bindingSource.Filter = string.IsNullOrEmpty(filter) ? null : $"Название LIKE '%{filter}%' OR Автор LIKE '%{filter}%'";
            UpdateCount();
        }

        private void ClearFilter()
        {
            txtFilter.Text = "";
            bindingSource.Filter = null;
            UpdateCount();
        }

        private void AddBook()
        {
            DataRow newRow = booksTable.NewRow();
            newRow["ID"] = nextId++;
            newRow["Название"] = "Новая книга";
            newRow["Автор"] = "Автор";
            newRow["Год"] = DateTime.Now.Year;
            newRow["Цена"] = 0;
            booksTable.Rows.Add(newRow);
            bindingSource.Position = bindingSource.Count - 1;
            UpdateCount();
        }

        private void DeleteBook()
        {
            if (bindingSource.Current != null && MessageBox.Show("Удалить книгу?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                bindingSource.RemoveCurrent();
                UpdateCount();
            }
        }

        private void UpdateCount()
        {
            lblCount.Text = $"Записей: {bindingSource.Count}";
        }
    }
}