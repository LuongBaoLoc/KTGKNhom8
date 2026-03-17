using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using KTGKNhom8.Authorization.Roles;
using KTGKNhom8.Authorization.Users;
using KTGKNhom8.MultiTenancy;
using KTGKNhom8.ToeicExams;

namespace KTGKNhom8.EntityFrameworkCore
{
    public class KTGKNhom8DbContext : AbpZeroDbContext<Tenant, Role, User, KTGKNhom8DbContext>
    {
        /* Define a DbSet for each entity of the application */
        
        // Thêm 4 bảng quản lý thi TOEIC vào đây:
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamPart> ExamParts { get; set; }
        public DbSet<Passage> Passages { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        
        public KTGKNhom8DbContext(DbContextOptions<KTGKNhom8DbContext> options)
            : base(options)
        {
            
        }
    }
}