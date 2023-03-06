alter table service_providers add constraint pk_service_providers primary key (id);

create table subjects (
	id bigserial,
	msisdn varchar(15) not null,
	service_provider_id integer not null,
	subject_id uuid not null,
	created_at timestamp with time zone not null,
	updated_at timestamp with time zone,

	constraint pk_subjects primary key (id),
	constraint fk_subjects_service_provider_id foreign key (service_provider_id)
		references service_providers (id)
);

create index ix_subjects_msisdn_service_provider_id on subjects (msisdn, service_provider_id);
