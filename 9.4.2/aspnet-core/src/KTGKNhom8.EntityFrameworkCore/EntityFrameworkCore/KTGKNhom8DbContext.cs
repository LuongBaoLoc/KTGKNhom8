using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using KTGKNhom8.Authorization.Roles;
using KTGKNhom8.Authorization.Users;
using KTGKNhom8.MultiTenancy;
using KTGKNHOM8.Toeic;

namespace KTGKNhom8.EntityFrameworkCore
{
    public class KTGKNhom8DbContext : AbpZeroDbContext<Tenant, Role, User, KTGKNhom8DbContext>
    {
        /* Define a DbSet for each entity of the application */
        
        // TOEIC Reading Test tables
        public DbSet<KTGKNHOM8.Toeic.Exam> Exams { get; set; }
        public DbSet<KTGKNHOM8.Toeic.ExamPart> ExamParts { get; set; }
        public DbSet<KTGKNHOM8.Toeic.Passage> Passages { get; set; }
        public DbSet<KTGKNHOM8.Toeic.Question> Questions { get; set; }
        public DbSet<KTGKNHOM8.Toeic.Answer> Answers { get; set; }
        public DbSet<KTGKNHOM8.Toeic.ExamAttempt> ExamAttempts { get; set; }
        public DbSet<KTGKNHOM8.Toeic.ExamAttemptAnswer> ExamAttemptAnswers { get; set; }
        
        public KTGKNhom8DbContext(DbContextOptions<KTGKNhom8DbContext> options)
            : base(options)
        {
            
        }
    }
}