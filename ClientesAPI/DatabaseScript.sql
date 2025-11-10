-- =============================================
-- Script de Creación de Base de Datos - Clientes API
-- Lenguajes Visuales II - Segundo Parcial
-- =============================================

-- Crear la base de datos si no existe
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ClientesDB')
BEGIN
    CREATE DATABASE ClientesDB;
END
GO

USE ClientesDB;
GO

-- =============================================
-- Tabla: Clientes
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Clientes')
BEGIN
    CREATE TABLE Clientes (
        CI NVARCHAR(20) NOT NULL PRIMARY KEY,
        Nombres NVARCHAR(200) NOT NULL,
        Direccion NVARCHAR(500) NOT NULL,
        Telefono NVARCHAR(50) NOT NULL,
        FotoCasa1 VARBINARY(MAX) NULL,
        FotoCasa2 VARBINARY(MAX) NULL,
        FotoCasa3 VARBINARY(MAX) NULL,
        FechaRegistro DATETIME NOT NULL DEFAULT GETDATE()
    );
    
    PRINT 'Tabla Clientes creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla Clientes ya existe';
END
GO

-- =============================================
-- Tabla: ArchivoCliente
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ArchivosCliente')
BEGIN
    CREATE TABLE ArchivosCliente (
        IdArchivo INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CICliente NVARCHAR(20) NOT NULL,
        NombreArchivo NVARCHAR(255) NOT NULL,
        UrlArchivo NVARCHAR(500) NOT NULL,
        Extension NVARCHAR(10) NULL,
        TamanoBytes BIGINT NOT NULL DEFAULT 0,
        FechaSubida DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_ArchivosCliente_Clientes FOREIGN KEY (CICliente) 
            REFERENCES Clientes(CI) ON DELETE CASCADE
    );
    
    -- Crear índices para mejorar rendimiento
    CREATE INDEX IX_ArchivosCliente_CICliente ON ArchivosCliente(CICliente);
    
    PRINT 'Tabla ArchivosCliente creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla ArchivosCliente ya existe';
END
GO

-- =============================================
-- Tabla: LogsApi
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LogsApi')
BEGIN
    CREATE TABLE LogsApi (
        IdLog INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        DateTime DATETIME NOT NULL DEFAULT GETDATE(),
        TipoLog NVARCHAR(50) NOT NULL,
        RequestBody NVARCHAR(MAX) NULL,
        ResponseBody NVARCHAR(MAX) NULL,
        UrlEndpoint NVARCHAR(500) NULL,
        MetodoHttp NVARCHAR(10) NULL,
        DireccionIp NVARCHAR(50) NULL,
        Detalle NVARCHAR(MAX) NULL,
        StatusCode INT NOT NULL DEFAULT 0,
        DuracionMs FLOAT NOT NULL DEFAULT 0
    );
    
    -- Crear índices para mejorar consultas
    CREATE INDEX IX_LogsApi_DateTime ON LogsApi(DateTime DESC);
    CREATE INDEX IX_LogsApi_TipoLog ON LogsApi(TipoLog);
    
    PRINT 'Tabla LogsApi creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla LogsApi ya existe';
END
GO

-- =============================================
-- Datos de prueba (Opcional)
-- =============================================
-- Insertar cliente de prueba
IF NOT EXISTS (SELECT * FROM Clientes WHERE CI = '1234567890')
BEGIN
    INSERT INTO Clientes (CI, Nombres, Direccion, Telefono, FechaRegistro)
    VALUES ('1234567890', 'Juan Pérez Gómez', 'Av. Principal #123, Santa Cruz', '77777777', GETDATE());
    
    PRINT 'Cliente de prueba insertado';
END
GO

-- =============================================
-- Verificación
-- =============================================
PRINT '========================================';
PRINT 'Verificación de tablas creadas:';
PRINT '========================================';

SELECT 
    t.name AS NombreTabla,
    SUM(p.rows) AS NumeroFilas
FROM 
    sys.tables t
INNER JOIN      
    sys.partitions p ON t.object_id = p.object_id
WHERE 
    t.name IN ('Clientes', 'ArchivosCliente', 'LogsApi')
    AND p.index_id IN (0,1)
GROUP BY 
    t.name
ORDER BY 
    t.name;

PRINT '========================================';
PRINT 'Base de datos configurada exitosamente';
PRINT '========================================';
GO
