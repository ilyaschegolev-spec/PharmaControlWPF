using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PharmaControlWPF
{
    public partial class MainWindow : Window
    {
        // Данные (заглушки)
        private List<Patient> patients;
        private List<Drug> drugs;
        private List<Interaction> interactions;
        private List<Prescription> prescriptions;
        private List<ComplianceRecord> complianceRecords;
        private Patient currentPatient;
        private string currentRole;

        public MainWindow()
        {
            InitializeComponent();
            InitializeMockData();
            currentRole = "Врач";
            RefreshUI();
        }

        private void InitializeMockData()
        {
            patients = new List<Patient>();
            drugs = new List<Drug>();
            interactions = new List<Interaction>();
            prescriptions = new List<Prescription>();
            complianceRecords = new List<ComplianceRecord>();

            // Пациенты
            patients.Add(new Patient { Id = 1, FullName = "Иванов Иван Иванович", AllergySubstances = new List<string> { "Пенициллин" } });
            patients.Add(new Patient { Id = 2, FullName = "Петрова Анна Сергеевна", AllergySubstances = new List<string> { "Аспирин" } });
            patients.Add(new Patient { Id = 3, FullName = "Сидоров Петр Алексеевич", AllergySubstances = new List<string>() });

            // Препараты
            drugs.Add(new Drug { Id = 1, Name = "Амоксициллин", Substance = "Пенициллин", Instruction = "Антибиотик, принимать после еды. Курс - не менее 5 дней." });
            drugs.Add(new Drug { Id = 2, Name = "Аспирин", Substance = "Ацетилсалициловая кислота", Instruction = "Принимать после еды, запивать водой. Не принимать на голодный желудок." });
            drugs.Add(new Drug { Id = 3, Name = "Парацетамол", Substance = "Парацетамол", Instruction = "Не более 4 г в сутки. При температуре выше 38.5°C." });
            drugs.Add(new Drug { Id = 4, Name = "Тетрациклин", Substance = "Тетрациклин", Instruction = "Антибиотик широкого спектра. Не принимать с молочными продуктами." });

            // Взаимодействия
            interactions.Add(new Interaction { Substance1 = "Пенициллин", Substance2 = "Тетрациклин", DangerLevel = "Желтый", Effect = "Снижение эффективности антибиотиков" });
            interactions.Add(new Interaction { Substance1 = "Аспирин", Substance2 = "Варфарин", DangerLevel = "Красный", Effect = "Повышенный риск кровотечений" });
            interactions.Add(new Interaction { Substance1 = "Парацетамол", Substance2 = "Алкоголь", DangerLevel = "Красный", Effect = "Токсическое поражение печени" });

            // Назначения для пациента 1 (активные)
            prescriptions.Add(new Prescription
            {
                Id = 1,
                PatientId = 1,
                DrugName = "Амоксициллин",
                Dosage = "500 мг",
                Frequency = "2 раза в день",
                DurationDays = 7,
                StartDate = DateTime.Now.AddDays(-2),
                IsActive = true
            });

            // Назначения для пациента 1 (завершенные)
            prescriptions.Add(new Prescription
            {
                Id = 2,
                PatientId = 1,
                DrugName = "Аспирин",
                Dosage = "100 мг",
                Frequency = "1 раз в день",
                DurationDays = 5,
                StartDate = DateTime.Now.AddDays(-10),
                IsActive = false
            });

            // Назначения для пациента 2
            prescriptions.Add(new Prescription
            {
                Id = 3,
                PatientId = 2,
                DrugName = "Парацетамол",
                Dosage = "500 мг",
                Frequency = "3 раза в день",
                DurationDays = 3,
                StartDate = DateTime.Now.AddDays(-1),
                IsActive = true
            });
        }

        private void RefreshUI()
        {
            lblRole.Text = $"Роль: {(currentRole == "Врач" ? "Врач" : "Пациент")}";
            lblStatus.Text = $"Текущий пациент: {(currentPatient?.FullName ?? "не выбран")}";

            if (currentRole == "Врач")
            {
                tabControlMain.SelectedItem = tabDoctor;
                RefreshPatientList();
                if (currentPatient != null)
                    RefreshPrescriptionsForDoctor();
            }
            else
            {
                tabControlMain.SelectedItem = tabPatient;
                if (currentPatient == null && patients.Count > 0)
                    currentPatient = patients[0];
                RefreshForPatient();
            }
        }

        private void RefreshPatientList()
        {
            listBoxPatients.ItemsSource = null;
            listBoxPatients.ItemsSource = patients;
        }

        private void RefreshPrescriptionsForDoctor()
        {
            if (currentPatient == null) return;

            var active = prescriptions.Where(p => p.PatientId == currentPatient.Id && p.IsActive)
                .Select(p => new { p.Id, p.DrugName, p.Dosage, p.Frequency, p.DurationDays, StartDate = p.StartDate.ToShortDateString() })
                .ToList();
            dataGridActivePrescriptions.ItemsSource = active;

            var history = prescriptions.Where(p => p.PatientId == currentPatient.Id && !p.IsActive).ToList();
            listBoxHistory.ItemsSource = history;
        }

        private void RefreshForPatient()
        {
            if (currentPatient == null) return;

            lblPatientName.Text = $"Пациент: {currentPatient.FullName}";

            // расписание на сегодня (активные назначения)
            var todayPrescriptions = prescriptions.Where(p => p.PatientId == currentPatient.Id && p.IsActive).ToList();
            listBoxSchedule.ItemsSource = todayPrescriptions.Select(p => $"{p.DrugName} - {p.Dosage}, {p.Frequency}");

            // завершенные курсы
            var completed = prescriptions.Where(p => p.PatientId == currentPatient.Id && !p.IsActive).ToList();
            listBoxCompletedCourses.ItemsSource = completed.Select(p => $"{p.DrugName} (завершен {p.StartDate.AddDays(p.DurationDays):dd.MM.yyyy})");

            // архив отметок
            var marks = complianceRecords.Where(r => r.PatientId == currentPatient.Id)
                .Select(r => new { r.DrugName, r.Status, r.DateTime })
                .ToList();
            dataGridMarks.ItemsSource = marks;
        }

        // === Обработчики событий ===

        private void ListBoxPatients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBoxPatients.SelectedItem != null)
            {
                currentPatient = listBoxPatients.SelectedItem as Patient;
                RefreshPrescriptionsForDoctor();
                lblStatus.Text = $"Выбран пациент: {currentPatient?.FullName}";
            }
        }

        private void BtnCheckCompatibility_Click(object sender, RoutedEventArgs e)
        {
            if (currentPatient == null)
            {
                MessageBox.Show("Сначала выберите пациента", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string newDrugSubstance = "Тетрациклин";
            var activeSubstances = prescriptions
                .Where(p => p.PatientId == currentPatient.Id && p.IsActive)
                .Select(p => drugs.FirstOrDefault(d => d.Name == p.DrugName)?.Substance)
                .Where(s => s != null)
                .ToList();

            var conflicts = interactions.Where(i =>
                (activeSubstances.Contains(i.Substance1) && i.Substance2 == newDrugSubstance) ||
                (activeSubstances.Contains(i.Substance2) && i.Substance1 == newDrugSubstance)).ToList();

            if (conflicts.Any())
            {
                string msg = "⚠️ Найдены взаимодействия:\n\n";
                foreach (var c in conflicts)
                    msg += $"• {c.Substance1} + {c.Substance2}\n  Уровень: {c.DangerLevel}\n  Эффект: {c.Effect}\n\n";
                MessageBox.Show(msg, "Совместимость", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show("✅ Совместимость подтверждена. Взаимодействий не обнаружено.", "Совместимость", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnCheckAllergy_Click(object sender, RoutedEventArgs e)
        {
            if (currentPatient == null)
            {
                MessageBox.Show("Сначала выберите пациента", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string newDrugSubstance = "Пенициллин";
            if (currentPatient.AllergySubstances.Contains(newDrugSubstance))
            {
                MessageBox.Show($"⚠️ ВНИМАНИЕ! У пациента аллергия на {newDrugSubstance}!", "Аллергия", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show($"✅ Аллергии на {newDrugSubstance} не обнаружено.", "Аллергия", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnAddPrescription_Click(object sender, RoutedEventArgs e)
        {
            if (currentPatient == null)
            {
                MessageBox.Show("Сначала выберите пациента", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Простое диалоговое окно для добавления
            var dialog = new AddPrescriptionDialog(drugs);
            if (dialog.ShowDialog() == true)
            {
                var newPrescription = new Prescription
                {
                    Id = prescriptions.Count + 1,
                    PatientId = currentPatient.Id,
                    DrugName = dialog.SelectedDrugName,
                    Dosage = dialog.Dosage,
                    Frequency = dialog.Frequency,
                    DurationDays = dialog.DurationDays,
                    StartDate = DateTime.Now,
                    IsActive = true
                };
                prescriptions.Add(newPrescription);
                RefreshPrescriptionsForDoctor();
                MessageBox.Show("✅ Препарат добавлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDeleteDrug_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridActivePrescriptions.SelectedItem != null)
            {
                dynamic selected = dataGridActivePrescriptions.SelectedItem;
                int id = selected.Id;
                var toDelete = prescriptions.FirstOrDefault(p => p.Id == id);
                if (toDelete != null)
                {
                    prescriptions.Remove(toDelete);
                    RefreshPrescriptionsForDoctor();
                    MessageBox.Show("✅ Препарат удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Выберите препарат в таблице", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnAddInteraction_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddInteractionDialog();
            if (dialog.ShowDialog() == true)
            {
                interactions.Add(new Interaction
                {
                    Substance1 = dialog.Substance1,
                    Substance2 = dialog.Substance2,
                    DangerLevel = dialog.DangerLevel,
                    Effect = dialog.Effect
                });
                MessageBox.Show("✅ Взаимодействие добавлено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnLowComplianceReport_Click(object sender, RoutedEventArgs e)
        {
            var nonCompliant = complianceRecords
                .Where(r => r.Status == "Пропустил")
                .Select(r => patients.FirstOrDefault(p => p.Id == r.PatientId)?.FullName)
                .Where(n => n != null)
                .Distinct()
                .ToList();

            if (nonCompliant.Any())
            {
                string list = string.Join("\n", nonCompliant);
                MessageBox.Show($"📊 Пациенты с низкой приверженностью:\n\n{list}", "Отчет", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("📊 Нет пациентов с пропусками приема", "Отчет", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnExportReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("📁 Отчет выгружен в файл report.txt (демо)", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnMarkTaken_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxSchedule.SelectedItem != null)
            {
                string selectedDrug = listBoxSchedule.SelectedItem.ToString();
                string drugName = selectedDrug.Split('-')[0].Trim();

                complianceRecords.Add(new ComplianceRecord
                {
                    PatientId = currentPatient.Id,
                    DrugName = drugName,
                    Status = "Принял",
                    DateTime = DateTime.Now
                });

                MessageBox.Show($"✅ Отметка 'Принял' для {drugName} записана", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshForPatient();
            }
            else
            {
                MessageBox.Show("Выберите препарат из расписания", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnMarkMissed_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxSchedule.SelectedItem != null)
            {
                string selectedDrug = listBoxSchedule.SelectedItem.ToString();
                string drugName = selectedDrug.Split('-')[0].Trim();

                complianceRecords.Add(new ComplianceRecord
                {
                    PatientId = currentPatient.Id,
                    DrugName = drugName,
                    Status = "Пропустил",
                    DateTime = DateTime.Now
                });

                MessageBox.Show($"⚠️ Отметка 'Пропустил' для {drugName} записана", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshForPatient();
            }
            else
            {
                MessageBox.Show("Выберите препарат из расписания", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnShowDrugInfo_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxSchedule.SelectedItem != null)
            {
                string selectedDrug = listBoxSchedule.SelectedItem.ToString();
                string drugName = selectedDrug.Split('-')[0].Trim();

                var drug = drugs.FirstOrDefault(d => d.Name == drugName);
                if (drug != null)
                {
                    txtInstruction.Text = drug.Instruction;
                }
                else
                {
                    txtInstruction.Text = "Инструкция не найдена";
                }
            }
            else
            {
                MessageBox.Show("Выберите препарат из расписания", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnSwitchRole_Click(object sender, RoutedEventArgs e)
        {
            currentRole = currentRole == "Врач" ? "Пациент" : "Врач";
            if (currentRole == "Пациент" && currentPatient == null && patients.Count > 0)
                currentPatient = patients[0];
            RefreshUI();
        }
    }

    // ==================== МОДЕЛИ ДАННЫХ ====================

    public class Patient
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public List<string> AllergySubstances { get; set; }
    }

    public class Drug
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Substance { get; set; }
        public string Instruction { get; set; }
    }

    public class Interaction
    {
        public string Substance1 { get; set; }
        public string Substance2 { get; set; }
        public string DangerLevel { get; set; }
        public string Effect { get; set; }
    }

    public class Prescription
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string DrugName { get; set; }
        public string Dosage { get; set; }
        public string Frequency { get; set; }
        public int DurationDays { get; set; }
        public DateTime StartDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class ComplianceRecord
    {
        public int PatientId { get; set; }
        public string DrugName { get; set; }
        public string Status { get; set; }
        public DateTime DateTime { get; set; }
    }

    // ==================== ДИАЛОГ ДОБАВЛЕНИЯ ПРЕПАРАТА ====================

    public class AddPrescriptionDialog : Window
    {
        public string SelectedDrugName { get; private set; }
        public string Dosage { get; private set; }
        public string Frequency { get; private set; }
        public int DurationDays { get; private set; }

        private ComboBox cmbDrug;
        private TextBox txtDosage;
        private TextBox txtFrequency;
        private TextBox txtDuration; // Вместо NumericUpDown используем TextBox

        public AddPrescriptionDialog(List<Drug> drugs)
        {
            Title = "Добавление препарата";
            Width = 400;
            Height = 320;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Препарат
            var lblDrug = new TextBlock { Text = "Препарат:", Margin = new Thickness(0, 10, 0, 0) };
            Grid.SetRow(lblDrug, 0);
            grid.Children.Add(lblDrug);

            cmbDrug = new ComboBox { Margin = new Thickness(0, 35, 0, 0), Height = 30 };
            foreach (var drug in drugs)
                cmbDrug.Items.Add(drug.Name);
            if (cmbDrug.Items.Count > 0) cmbDrug.SelectedIndex = 0;
            Grid.SetRow(cmbDrug, 0);
            grid.Children.Add(cmbDrug);

            // Дозировка
            var lblDosage = new TextBlock { Text = "Дозировка:", Margin = new Thickness(0, 10, 0, 0) };
            Grid.SetRow(lblDosage, 1);
            grid.Children.Add(lblDosage);

            txtDosage = new TextBox { Text = "500 мг", Margin = new Thickness(0, 35, 0, 0), Height = 30 };
            Grid.SetRow(txtDosage, 1);
            grid.Children.Add(txtDosage);

            // Кратность
            var lblFrequency = new TextBlock { Text = "Кратность:", Margin = new Thickness(0, 10, 0, 0) };
            Grid.SetRow(lblFrequency, 2);
            grid.Children.Add(lblFrequency);

            txtFrequency = new TextBox { Text = "2 раза в день", Margin = new Thickness(0, 35, 0, 0), Height = 30 };
            Grid.SetRow(txtFrequency, 2);
            grid.Children.Add(txtFrequency);

            // Длительность
            var lblDuration = new TextBlock { Text = "Длительность (дни):", Margin = new Thickness(0, 10, 0, 0) };
            Grid.SetRow(lblDuration, 3);
            grid.Children.Add(lblDuration);

            txtDuration = new TextBox { Text = "7", Margin = new Thickness(0, 35, 0, 0), Height = 30 };
            Grid.SetRow(txtDuration, 3);
            grid.Children.Add(txtDuration);

            // Кнопки
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var btnOk = new Button { Content = "OK", Width = 80, Height = 30, Margin = new Thickness(0, 0, 10, 0) };
            btnOk.Click += (s, e) => { SaveAndClose(); };

            var btnCancel = new Button { Content = "Отмена", Width = 80, Height = 30 };
            btnCancel.Click += (s, e) => { DialogResult = false; Close(); };

            buttonPanel.Children.Add(btnOk);
            buttonPanel.Children.Add(btnCancel);
            Grid.SetRow(buttonPanel, 4);
            grid.Children.Add(buttonPanel);

            Content = grid;
        }

        private void SaveAndClose()
        {
            SelectedDrugName = cmbDrug.SelectedItem?.ToString();
            Dosage = txtDosage.Text;
            Frequency = txtFrequency.Text;

            // Проверка корректности ввода длительности
            if (!int.TryParse(txtDuration.Text, out int duration) || duration <= 0 || duration > 365)
            {
                MessageBox.Show("Введите корректную длительность курса (число от 1 до 365)",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DurationDays = duration;

            if (string.IsNullOrEmpty(SelectedDrugName))
            {
                MessageBox.Show("Выберите препарат", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }
    }

    // ==================== ДИАЛОГ ДОБАВЛЕНИЯ ВЗАИМОДЕЙСТВИЯ (ИСПРАВЛЕННЫЙ) ====================

    public class AddInteractionDialog : Window
    {
        public string Substance1 { get; private set; }
        public string Substance2 { get; private set; }
        public string DangerLevel { get; private set; }
        public string Effect { get; private set; }

        private TextBox txtSubstance1;
        private TextBox txtSubstance2;
        private ComboBox cmbDanger;
        private TextBox txtEffect;

        public AddInteractionDialog()
        {
            Title = "Добавление взаимодействия";
            Width = 450;
            Height = 320;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Вещество 1
            var lbl1 = new TextBlock { Text = "Первое вещество:", Margin = new Thickness(0, 10, 0, 0) };
            Grid.SetRow(lbl1, 0);
            grid.Children.Add(lbl1);

            txtSubstance1 = new TextBox { Text = "Пенициллин", Margin = new Thickness(0, 35, 0, 0), Height = 30 };
            Grid.SetRow(txtSubstance1, 0);
            grid.Children.Add(txtSubstance1);

            // Вещество 2
            var lbl2 = new TextBlock { Text = "Второе вещество:", Margin = new Thickness(0, 10, 0, 0) };
            Grid.SetRow(lbl2, 1);
            grid.Children.Add(lbl2);

            txtSubstance2 = new TextBox { Text = "Тетрациклин", Margin = new Thickness(0, 35, 0, 0), Height = 30 };
            Grid.SetRow(txtSubstance2, 1);
            grid.Children.Add(txtSubstance2);

            // Уровень опасности
            var lbl3 = new TextBlock { Text = "Уровень опасности:", Margin = new Thickness(0, 10, 0, 0) };
            Grid.SetRow(lbl3, 2);
            grid.Children.Add(lbl3);

            cmbDanger = new ComboBox { Margin = new Thickness(0, 35, 0, 0), Height = 30 };
            cmbDanger.Items.Add("Красный");
            cmbDanger.Items.Add("Желтый");
            cmbDanger.SelectedIndex = 1;
            Grid.SetRow(cmbDanger, 2);
            grid.Children.Add(cmbDanger);

            // Эффект
            var lbl4 = new TextBlock { Text = "Описание эффекта:", Margin = new Thickness(0, 10, 0, 0) };
            Grid.SetRow(lbl4, 3);
            grid.Children.Add(lbl4);

            txtEffect = new TextBox { Text = "Снижение эффективности", Margin = new Thickness(0, 35, 0, 0), Height = 30 };
            Grid.SetRow(txtEffect, 3);
            grid.Children.Add(txtEffect);

            // Кнопки
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var btnOk = new Button { Content = "OK", Width = 80, Height = 30, Margin = new Thickness(0, 0, 10, 0) };
            btnOk.Click += (s, e) => { SaveAndClose(); };

            var btnCancel = new Button { Content = "Отмена", Width = 80, Height = 30 };
            btnCancel.Click += (s, e) => { DialogResult = false; Close(); };

            buttonPanel.Children.Add(btnOk);
            buttonPanel.Children.Add(btnCancel);
            Grid.SetRow(buttonPanel, 4);
            grid.Children.Add(buttonPanel);

            Content = grid;
        }

        private void SaveAndClose()
        {
            Substance1 = txtSubstance1.Text.Trim();
            Substance2 = txtSubstance2.Text.Trim();
            DangerLevel = cmbDanger.SelectedItem?.ToString();
            Effect = txtEffect.Text.Trim();

            if (string.IsNullOrEmpty(Substance1) || string.IsNullOrEmpty(Substance2))
            {
                MessageBox.Show("Вещества не могут быть пустыми", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}