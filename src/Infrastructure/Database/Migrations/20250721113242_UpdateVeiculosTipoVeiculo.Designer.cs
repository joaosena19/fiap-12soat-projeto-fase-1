﻿// <auto-generated />
using System;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250721113242_UpdateVeiculosTipoVeiculo")]
    partial class UpdateVeiculosTipoVeiculo
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Domain.Cadastros.Aggregates.Cliente", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.HasKey("Id");

                    b.ToTable("clientes", (string)null);
                });

            modelBuilder.Entity("Domain.Cadastros.Aggregates.Servico", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.HasKey("Id");

                    b.ToTable("servicos", (string)null);
                });

            modelBuilder.Entity("Domain.Cadastros.Aggregates.Veiculo", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.HasKey("Id");

                    b.ToTable("veiculos", (string)null);
                });

            modelBuilder.Entity("Domain.Cadastros.Aggregates.Cliente", b =>
                {
                    b.OwnsOne("Domain.Cadastros.ValueObjects.Cliente.Cpf", "Cpf", b1 =>
                        {
                            b1.Property<Guid>("ClienteId")
                                .HasColumnType("uuid");

                            b1.Property<string>("Valor")
                                .IsRequired()
                                .HasMaxLength(11)
                                .HasColumnType("character varying(11)")
                                .HasColumnName("cpf");

                            b1.HasKey("ClienteId");

                            b1.ToTable("clientes");

                            b1.WithOwner()
                                .HasForeignKey("ClienteId");
                        });

                    b.OwnsOne("Domain.Cadastros.ValueObjects.Cliente.NomeCliente", "Nome", b1 =>
                        {
                            b1.Property<Guid>("ClienteId")
                                .HasColumnType("uuid");

                            b1.Property<string>("Valor")
                                .IsRequired()
                                .HasMaxLength(200)
                                .HasColumnType("character varying(200)")
                                .HasColumnName("nome");

                            b1.HasKey("ClienteId");

                            b1.ToTable("clientes");

                            b1.WithOwner()
                                .HasForeignKey("ClienteId");
                        });

                    b.Navigation("Cpf")
                        .IsRequired();

                    b.Navigation("Nome")
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Cadastros.Aggregates.Servico", b =>
                {
                    b.OwnsOne("Domain.Cadastros.ValueObjects.Servico.NomeServico", "Nome", b1 =>
                        {
                            b1.Property<Guid>("ServicoId")
                                .HasColumnType("uuid");

                            b1.Property<string>("Valor")
                                .IsRequired()
                                .HasMaxLength(500)
                                .HasColumnType("character varying(500)")
                                .HasColumnName("nome");

                            b1.HasKey("ServicoId");

                            b1.ToTable("servicos");

                            b1.WithOwner()
                                .HasForeignKey("ServicoId");
                        });

                    b.OwnsOne("Domain.Cadastros.ValueObjects.Servico.PrecoServico", "Preco", b1 =>
                        {
                            b1.Property<Guid>("ServicoId")
                                .HasColumnType("uuid");

                            b1.Property<decimal>("Valor")
                                .HasColumnType("decimal(18,2)")
                                .HasColumnName("preco");

                            b1.HasKey("ServicoId");

                            b1.ToTable("servicos");

                            b1.WithOwner()
                                .HasForeignKey("ServicoId");
                        });

                    b.Navigation("Nome")
                        .IsRequired();

                    b.Navigation("Preco")
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Cadastros.Aggregates.Veiculo", b =>
                {
                    b.OwnsOne("Domain.Cadastros.ValueObjects.Veiculo.Ano", "Ano", b1 =>
                        {
                            b1.Property<Guid>("VeiculoId")
                                .HasColumnType("uuid");

                            b1.Property<int>("Valor")
                                .HasColumnType("integer")
                                .HasColumnName("ano");

                            b1.HasKey("VeiculoId");

                            b1.ToTable("veiculos");

                            b1.WithOwner()
                                .HasForeignKey("VeiculoId");
                        });

                    b.OwnsOne("Domain.Cadastros.ValueObjects.Veiculo.Cor", "Cor", b1 =>
                        {
                            b1.Property<Guid>("VeiculoId")
                                .HasColumnType("uuid");

                            b1.Property<string>("Valor")
                                .IsRequired()
                                .HasMaxLength(100)
                                .HasColumnType("character varying(100)")
                                .HasColumnName("cor");

                            b1.HasKey("VeiculoId");

                            b1.ToTable("veiculos");

                            b1.WithOwner()
                                .HasForeignKey("VeiculoId");
                        });

                    b.OwnsOne("Domain.Cadastros.ValueObjects.Veiculo.Marca", "Marca", b1 =>
                        {
                            b1.Property<Guid>("VeiculoId")
                                .HasColumnType("uuid");

                            b1.Property<string>("Valor")
                                .IsRequired()
                                .HasMaxLength(200)
                                .HasColumnType("character varying(200)")
                                .HasColumnName("marca");

                            b1.HasKey("VeiculoId");

                            b1.ToTable("veiculos");

                            b1.WithOwner()
                                .HasForeignKey("VeiculoId");
                        });

                    b.OwnsOne("Domain.Cadastros.ValueObjects.Veiculo.Modelo", "Modelo", b1 =>
                        {
                            b1.Property<Guid>("VeiculoId")
                                .HasColumnType("uuid");

                            b1.Property<string>("Valor")
                                .IsRequired()
                                .HasMaxLength(200)
                                .HasColumnType("character varying(200)")
                                .HasColumnName("modelo");

                            b1.HasKey("VeiculoId");

                            b1.ToTable("veiculos");

                            b1.WithOwner()
                                .HasForeignKey("VeiculoId");
                        });

                    b.OwnsOne("Domain.Cadastros.ValueObjects.Veiculo.Placa", "Placa", b1 =>
                        {
                            b1.Property<Guid>("VeiculoId")
                                .HasColumnType("uuid");

                            b1.Property<string>("Valor")
                                .IsRequired()
                                .HasMaxLength(7)
                                .HasColumnType("character varying(7)")
                                .HasColumnName("placa");

                            b1.HasKey("VeiculoId");

                            b1.ToTable("veiculos");

                            b1.WithOwner()
                                .HasForeignKey("VeiculoId");
                        });

                    b.OwnsOne("Domain.Cadastros.ValueObjects.Veiculo.TipoVeiculo", "TipoVeiculo", b1 =>
                        {
                            b1.Property<Guid>("VeiculoId")
                                .HasColumnType("uuid");

                            b1.Property<string>("Valor")
                                .IsRequired()
                                .HasMaxLength(10)
                                .HasColumnType("character varying(10)")
                                .HasColumnName("tipo_veiculo");

                            b1.HasKey("VeiculoId");

                            b1.ToTable("veiculos");

                            b1.WithOwner()
                                .HasForeignKey("VeiculoId");
                        });

                    b.Navigation("Ano")
                        .IsRequired();

                    b.Navigation("Cor")
                        .IsRequired();

                    b.Navigation("Marca")
                        .IsRequired();

                    b.Navigation("Modelo")
                        .IsRequired();

                    b.Navigation("Placa")
                        .IsRequired();

                    b.Navigation("TipoVeiculo")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
