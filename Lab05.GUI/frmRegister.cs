using Lab05.BUS;
using Lab05.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab05.GUI
{
    public partial class frmRegister : Form
    {
        private readonly StudentService studentService = new StudentService();
        private readonly FacultyService facultyService = new FacultyService();
        private readonly MajorService majorService = new MajorService();
        public frmRegister()
        {
            InitializeComponent();
        }

        private void frmRegister_Load(object sender, EventArgs e)
        {
            try
            {
                var listFacultys = facultyService.GetAll();
                FillFalcultyCombobox(listFacultys);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void FillFalcultyCombobox(List<Faculty> listFacultys)
        {
            this.cmbKhoa.DataSource = listFacultys;
            this.cmbKhoa.DisplayMember = "FacultyName";
            this.cmbKhoa.ValueMember = "FacultyID";
        }

        private void cmbKhoa_SelectedIndexChanged(object sender, EventArgs e)
        {
            Faculty selectedFaculty = cmbKhoa.SelectedItem as Faculty;
            if (selectedFaculty != null)
            {
                // Lấy danh sách các ngành (Major) theo FacultyID
                var listMajor = majorService.GetAllByFaculty(selectedFaculty.FacultyID);
                FillMajorCombobox(listMajor); // Đổ dữ liệu vào combobox chuyên ngành (Major)

                // Lấy danh sách sinh viên chưa có ngành (Major) theo FacultyID
                var listStudents = studentService.GetAllHasNoMajor(selectedFaculty.FacultyID);

                // Cập nhật lại danh sách sinh viên vào DataGridView
                BindGrid(listStudents);
            }
        }
        private void BindGrid(List<Student> listStudent)
        {
            dgvStudent.Rows.Clear(); // Xóa tất cả các hàng cũ trong DataGridView

            foreach (var item in listStudent)
            {
                int index = dgvStudent.Rows.Add(); // Thêm một dòng mới

                // Gán giá trị cho các cột trong DataGridView từ đối tượng Student
                dgvStudent.Rows[index].Cells[1].Value = item.StudentID; // Mã sinh viên
                dgvStudent.Rows[index].Cells[2].Value = item.FullName;   // Tên đầy đủ

                // Nếu Faculty không null, hiển thị tên khoa
                if (item.Faculty != null)
                {
                    dgvStudent.Rows[index].Cells[3].Value = item.Faculty.FacultyName;
                }

                // Hiển thị điểm trung bình (AverageScore)
                dgvStudent.Rows[index].Cells[4].Value = item.AverageScore.ToString();

                // Nếu MajorID không null, hiển thị tên ngành (Major)
                if (item.MajorID != null)
                {
                    dgvStudent.Rows[index].Cells[5].Value = item.Major.Name;
                }
            }
        }
        private void FillMajorCombobox(List<Major> listMajors)
        {
            this.cmbChuyenNganh.DataSource = listMajors;
            this.cmbChuyenNganh.DisplayMember = "MajorName";
            this.cmbChuyenNganh.ValueMember = "MajorID";
        }

        private void btnDangKy_Click(object sender, EventArgs e)
        {
            // Lấy MajorID từ combobox
            var selectedMajor = cmbMajor.SelectedItem as Major;
            if (selectedMajor == null)
            {
                MessageBox.Show("Vui lòng chọn chuyên ngành.");
                return;
            }

            // Lấy danh sách sinh viên được chọn
            var selectedStudents = new List<Student>();
            foreach (DataGridViewRow row in dgvStudents.Rows)
            {
                if (Convert.ToBoolean(row.Cells[0].Value)) // Kiểm tra nếu checkbox được chọn
                {
                    string studentID = row.Cells[1].Value.ToString();
                    var student = studentService.FindById(studentID);
                    if (student != null)
                    {
                        selectedStudents.Add(student);
                    }
                }
            }

            // Cập nhật MajorID cho các sinh viên được chọn
            foreach (var student in selectedStudents)
            {
                student.MajorID = selectedMajor.MajorID;
                studentService.InsertUpdate(student); // Lưu lại thay đổi vào DB
            }

            // Hiển thị thông báo
            MessageBox.Show("Đăng ký chuyên ngành thành công cho các sinh viên được chọn.");

            // Cập nhật lại DataGridView
            var listStudents = studentService.GetAllHasNoMajor(selectedFaculty.FacultyID);
            BindStudentGrid(listStudents);
        }
    }
}
