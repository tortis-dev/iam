using FluentMigrator;

namespace Tortis.Iam.Database;

[Migration(202508130001)]
public class CreateQuartzTables : Migration
{
    public override void Up()
    {
        // QRTZ_CALENDARS
        Create.Table("qrtz_calendars")
            .WithColumn("sched_name").AsString(120).NotNullable().PrimaryKey()
            .WithColumn("calendar_name").AsString(200).NotNullable().PrimaryKey()
            .WithColumn("calendar").AsBinary().NotNullable();

        // QRTZ_TRIGGERS
        Create.Table("qrtz_triggers")
            .WithColumn("sched_name").AsString(120).NotNullable().PrimaryKey()
            .WithColumn("trigger_name").AsString(150).NotNullable().PrimaryKey()
            .WithColumn("trigger_group").AsString(150).NotNullable().PrimaryKey()
            .WithColumn("job_name").AsString(150).NotNullable()
            .WithColumn("job_group").AsString(150).NotNullable()
            .WithColumn("description").AsString(250).Nullable()
            .WithColumn("next_fire_time").AsInt64().Nullable()
            .WithColumn("prev_fire_time").AsInt64().Nullable()
            .WithColumn("priority").AsInt32().Nullable()
            .WithColumn("trigger_state").AsString(16).NotNullable()
            .WithColumn("trigger_type").AsString(8).NotNullable()
            .WithColumn("start_time").AsInt64().NotNullable()
            .WithColumn("end_time").AsInt64().Nullable()
            .WithColumn("calendar_name").AsString(200).Nullable()
            .WithColumn("misfire_instr").AsInt16().Nullable()
            .WithColumn("job_data").AsBinary().Nullable();

        // QRTZ_JOB_DETAILS
        Create.Table("qrtz_job_details")
            .WithColumn("sched_name").AsString(120).NotNullable().PrimaryKey()
            .WithColumn("job_name").AsString(150).NotNullable().PrimaryKey()
            .WithColumn("job_group").AsString(150).NotNullable().PrimaryKey()
            .WithColumn("description").AsString(250).Nullable()
            .WithColumn("job_class_name").AsString(250).NotNullable()
            .WithColumn("is_durable").AsBoolean().NotNullable()
            .WithColumn("is_nonconcurrent").AsBoolean().NotNullable()
            .WithColumn("is_update_data").AsBoolean().NotNullable()
            .WithColumn("requests_recovery").AsBoolean().NotNullable()
            .WithColumn("job_data").AsBinary().Nullable();

        // QRTZ_SIMPLE_TRIGGERS
        Create.Table("qrtz_simple_triggers")
            .WithColumn("sched_name").AsString(120).NotNullable().PrimaryKey()
            .WithColumn("trigger_name").AsString(150).NotNullable().PrimaryKey()
            .WithColumn("trigger_group").AsString(150).NotNullable().PrimaryKey()
            .WithColumn("repeat_count").AsInt64().NotNullable()
            .WithColumn("repeat_interval").AsInt64().NotNullable()
            .WithColumn("times_triggered").AsInt64().NotNullable();

        // QRTZ_CRON_TRIGGERS
        Create.Table("qrtz_cron_triggers")
            .WithColumn("sched_name").AsString(120).NotNullable().PrimaryKey()
            .WithColumn("trigger_name").AsString(150).NotNullable().PrimaryKey()
            .WithColumn("trigger_group").AsString(150).NotNullable().PrimaryKey()
            .WithColumn("cron_expression").AsString(120).NotNullable()
            .WithColumn("time_zone_id").AsString(80).Nullable();

        // QRTZ_SIMPROP_TRIGGERS
        Create.Table("qrtz_simprop_triggers")
            .WithColumn("sched_name").AsString(120).NotNullable().PrimaryKey()
            .WithColumn("trigger_name").AsString(150).NotNullable().PrimaryKey()
            .WithColumn("trigger_group").AsString(150).NotNullable().PrimaryKey()
            .WithColumn("str_prop_1").AsString(512).Nullable()
            .WithColumn("str_prop_2").AsString(512).Nullable()
            .WithColumn("str_prop_3").AsString(512).Nullable()
            .WithColumn("int_prop_1").AsInt32().Nullable()
            .WithColumn("int_prop_2").AsInt32().Nullable()
            .WithColumn("long_prop_1").AsInt64().Nullable()
            .WithColumn("long_prop_2").AsInt64().Nullable()
            .WithColumn("dec_prop_1").AsDecimal().Nullable()
            .WithColumn("dec_prop_2").AsDecimal().Nullable()
            .WithColumn("bool_prop_1").AsBoolean().Nullable()
            .WithColumn("bool_prop_2").AsBoolean().Nullable();

        // QRTZ_BLOB_TRIGGERS
        Create.Table("qrtz_blob_triggers")
            .WithColumn("sched_name").AsString(120).NotNullable().PrimaryKey()
            .WithColumn("trigger_name").AsString(150).NotNullable().PrimaryKey()
            .WithColumn("trigger_group").AsString(150).NotNullable().PrimaryKey()
            .WithColumn("blob_data").AsBinary().Nullable();

        // QRTZ_FIRED_TRIGGERS
        Create.Table("qrtz_fired_triggers")
            .WithColumn("sched_name").AsString(120).NotNullable().PrimaryKey()
            .WithColumn("entry_id").AsString(140).NotNullable().PrimaryKey()
            .WithColumn("trigger_name").AsString(150).NotNullable()
            .WithColumn("trigger_group").AsString(150).NotNullable()
            .WithColumn("instance_name").AsString(200).NotNullable()
            .WithColumn("fired_time").AsInt64().NotNullable()
            .WithColumn("sched_time").AsInt64().NotNullable()
            .WithColumn("priority").AsInt32().NotNullable()
            .WithColumn("state").AsString(16).NotNullable()
            .WithColumn("job_name").AsString(150).Nullable()
            .WithColumn("job_group").AsString(150).Nullable()
            .WithColumn("is_nonconcurrent").AsBoolean().Nullable()
            .WithColumn("requests_recovery").AsBoolean().Nullable();

        // QRTZ_SCHEDULER_STATE
        Create.Table("qrtz_scheduler_state")
            .WithColumn("sched_name").AsString(120).NotNullable().PrimaryKey()
            .WithColumn("instance_name").AsString(200).NotNullable().PrimaryKey()
            .WithColumn("last_checkin_time").AsInt64().NotNullable()
            .WithColumn("checkin_interval").AsInt64().NotNullable();

        // QRTZ_LOCKS
        Create.Table("qrtz_locks")
            .WithColumn("sched_name").AsString(120).NotNullable().PrimaryKey()
            .WithColumn("lock_name").AsString(40).NotNullable().PrimaryKey();

        // QRTZ_PAUSED_TRIGGER_GRPS
        Create.Table("qrtz_paused_trigger_grps")
            .WithColumn("sched_name").AsString(120).NotNullable().PrimaryKey()
            .WithColumn("trigger_group").AsString(150).NotNullable().PrimaryKey();

        // Foreign Keys
        Create.ForeignKey("FK_QRTZ_TRIGGERS_QRTZ_JOB_DETAILS")
            .FromTable("qrtz_triggers").ForeignColumns("sched_name", "job_name", "job_group")
            .ToTable("qrtz_job_details").PrimaryColumns("sched_name", "job_name", "job_group");

        Create.ForeignKey("FK_QRTZ_SIMPLE_TRIGGERS_QRTZ_TRIGGERS")
            .FromTable("qrtz_simple_triggers").ForeignColumns("sched_name", "trigger_name", "trigger_group")
            .ToTable("qrtz_triggers").PrimaryColumns("sched_name", "trigger_name", "trigger_group");

        Create.ForeignKey("FK_QRTZ_CRON_TRIGGERS_QRTZ_TRIGGERS")
            .FromTable("qrtz_cron_triggers").ForeignColumns("sched_name", "trigger_name", "trigger_group")
            .ToTable("qrtz_triggers").PrimaryColumns("sched_name", "trigger_name", "trigger_group");

        Create.ForeignKey("FK_QRTZ_SIMPROP_TRIGGERS_QRTZ_TRIGGERS")
            .FromTable("qrtz_simprop_triggers").ForeignColumns("sched_name", "trigger_name", "trigger_group")
            .ToTable("qrtz_triggers").PrimaryColumns("sched_name", "trigger_name", "trigger_group");

        Create.ForeignKey("FK_QRTZ_BLOB_TRIGGERS_QRTZ_TRIGGERS")
            .FromTable("qrtz_blob_triggers").ForeignColumns("sched_name", "trigger_name", "trigger_group")
            .ToTable("qrtz_triggers").PrimaryColumns("sched_name", "trigger_name", "trigger_group");

        // Indexes
        Create.Index("IDX_QRTZ_T_J")
            .OnTable("qrtz_triggers")
            .OnColumn("sched_name").Ascending()
            .OnColumn("job_name").Ascending()
            .OnColumn("job_group");

        Create.Index("IDX_QRTZ_T_JG")
            .OnTable("qrtz_triggers")
            .OnColumn("sched_name").Ascending()
            .OnColumn("job_group");

        Create.Index("IDX_QRTZ_T_C")
            .OnTable("qrtz_triggers")
            .OnColumn("sched_name").Ascending()
            .OnColumn("calendar_name");

        Create.Index("IDX_QRTZ_T_G")
            .OnTable("qrtz_triggers")
            .OnColumn("sched_name").Ascending()
            .OnColumn("trigger_group");

        Create.Index("IDX_QRTZ_T_STATE")
            .OnTable("qrtz_triggers")
            .OnColumn("sched_name").Ascending()
            .OnColumn("trigger_state");

        Create.Index("IDX_QRTZ_T_N_STATE")
            .OnTable("qrtz_triggers")
            .OnColumn("sched_name").Ascending()
            .OnColumn("trigger_name").Ascending()
            .OnColumn("trigger_group").Ascending()
            .OnColumn("trigger_state");

        Create.Index("IDX_QRTZ_T_N_G_STATE")
            .OnTable("qrtz_triggers")
            .OnColumn("sched_name").Ascending()
            .OnColumn("trigger_group").Ascending()
            .OnColumn("trigger_state");

        Create.Index("IDX_QRTZ_T_NEXT_FIRE_TIME")
            .OnTable("qrtz_triggers")
            .OnColumn("sched_name").Ascending()
            .OnColumn("next_fire_time");

        Create.Index("IDX_QRTZ_T_NFT_ST")
            .OnTable("qrtz_triggers")
            .OnColumn("sched_name").Ascending()
            .OnColumn("trigger_state").Ascending()
            .OnColumn("next_fire_time");

        Create.Index("IDX_QRTZ_T_NFT_MISFIRE")
            .OnTable("qrtz_triggers")
            .OnColumn("sched_name").Ascending()
            .OnColumn("misfire_instr").Ascending()
            .OnColumn("next_fire_time");

        Create.Index("IDX_QRTZ_T_NFT_ST_MISFIRE")
            .OnTable("qrtz_triggers")
            .OnColumn("sched_name").Ascending()
            .OnColumn("misfire_instr").Ascending()
            .OnColumn("next_fire_time").Ascending()
            .OnColumn("trigger_state");

        Create.Index("IDX_QRTZ_T_NFT_ST_MISFIRE_GRP")
            .OnTable("qrtz_triggers")
            .OnColumn("sched_name").Ascending()
            .OnColumn("misfire_instr").Ascending()
            .OnColumn("next_fire_time").Ascending()
            .OnColumn("trigger_group").Ascending()
            .OnColumn("trigger_state");

        Create.Index("IDX_QRTZ_FT_TRIG_INST_NAME")
            .OnTable("qrtz_fired_triggers")
            .OnColumn("sched_name").Ascending()
            .OnColumn("instance_name");

        Create.Index("IDX_QRTZ_FT_INST_JOB_REQ_RCVRY")
            .OnTable("qrtz_fired_triggers")
            .OnColumn("sched_name").Ascending()
            .OnColumn("instance_name").Ascending()
            .OnColumn("requests_recovery");

        Create.Index("IDX_QRTZ_FT_J_G")
            .OnTable("qrtz_fired_triggers")
            .OnColumn("sched_name").Ascending()
            .OnColumn("job_name").Ascending()
            .OnColumn("job_group");

        Create.Index("IDX_QRTZ_FT_JG")
            .OnTable("qrtz_fired_triggers")
            .OnColumn("sched_name").Ascending()
            .OnColumn("job_group");

        Create.Index("IDX_QRTZ_FT_T_G")
            .OnTable("qrtz_fired_triggers")
            .OnColumn("sched_name").Ascending()
            .OnColumn("trigger_name").Ascending()
            .OnColumn("trigger_group");

        Create.Index("IDX_QRTZ_FT_TG")
            .OnTable("qrtz_fired_triggers")
            .OnColumn("sched_name").Ascending()
            .OnColumn("trigger_group");
    }

    public override void Down()
    {
        // Drop all tables in reverse order
        Delete.Table("qrtz_paused_trigger_grps");
        Delete.Table("qrtz_locks");
        Delete.Table("qrtz_scheduler_state");
        Delete.Table("qrtz_fired_triggers");
        Delete.Table("qrtz_blob_triggers");
        Delete.Table("qrtz_simprop_triggers");
        Delete.Table("qrtz_cron_triggers");
        Delete.Table("qrtz_simple_triggers");
        Delete.Table("qrtz_triggers");
        Delete.Table("qrtz_job_details");
        Delete.Table("qrtz_calendars");
    }
}