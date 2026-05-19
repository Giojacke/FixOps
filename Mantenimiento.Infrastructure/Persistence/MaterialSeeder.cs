using Mantenimiento.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mantenimiento.Infrastructure.Persistence;

public static class MaterialSeeder
{
    public static async Task SeedAsync(MantenimientoDbContext context)
    {
        if (await context.Set<Material>().AnyAsync()) return;

        var materiales = new List<Material>
        {
            // ── Plomería ──────────────────────────────────────────────────
            new() { Id=Guid.NewGuid(), Nombre="Llave de paso 1/2\"",               TipoMaterial="Plomería",          Descripcion="Llave de paso cromada para tubería de 1/2 pulgada",           PrecioUnitario=18500,  StockActual=30 },
            new() { Id=Guid.NewGuid(), Nombre="Llave de paso 3/4\"",               TipoMaterial="Plomería",          Descripcion="Llave de paso cromada para tubería de 3/4 pulgada",           PrecioUnitario=22000,  StockActual=25 },
            new() { Id=Guid.NewGuid(), Nombre="Llave de paso 1\"",                 TipoMaterial="Plomería",          Descripcion="Llave de paso cromada para tubería de 1 pulgada",             PrecioUnitario=28000,  StockActual=15 },
            new() { Id=Guid.NewGuid(), Nombre="Llave para lavamanos 4\"",          TipoMaterial="Plomería",          Descripcion="Llave mezcladora para lavamanos centros 4 pulgadas",          PrecioUnitario=85000,  StockActual=20 },
            new() { Id=Guid.NewGuid(), Nombre="Llave para lavamanos 8\"",          TipoMaterial="Plomería",          Descripcion="Llave mezcladora para lavamanos centros 8 pulgadas",          PrecioUnitario=110000, StockActual=15 },
            new() { Id=Guid.NewGuid(), Nombre="Llave mezcladora monocomando",      TipoMaterial="Plomería",          Descripcion="Llave monomando para lavamanos o fregadero",                  PrecioUnitario=145000, StockActual=12 },
            new() { Id=Guid.NewGuid(), Nombre="Llave de ducha",                    TipoMaterial="Plomería",          Descripcion="Llave mezcladora para ducha con brazo incluido",              PrecioUnitario=95000,  StockActual=10 },
            new() { Id=Guid.NewGuid(), Nombre="Tubo PVC 1/2\" x 3m",              TipoMaterial="Plomería",          Descripcion="Tubería PVC presión RDE 13.5, 1/2 pulgada, longitud 3m",     PrecioUnitario=12000,  StockActual=60 },
            new() { Id=Guid.NewGuid(), Nombre="Tubo PVC 3/4\" x 3m",              TipoMaterial="Plomería",          Descripcion="Tubería PVC presión RDE 13.5, 3/4 pulgada, longitud 3m",     PrecioUnitario=16000,  StockActual=50 },
            new() { Id=Guid.NewGuid(), Nombre="Tubo PVC 1\" x 3m",                TipoMaterial="Plomería",          Descripcion="Tubería PVC presión RDE 13.5, 1 pulgada, longitud 3m",       PrecioUnitario=22000,  StockActual=35 },
            new() { Id=Guid.NewGuid(), Nombre="Codo PVC 1/2\" 90°",               TipoMaterial="Plomería",          Descripcion="Codo PVC presión 90 grados de 1/2 pulgada",                  PrecioUnitario=1200,   StockActual=200 },
            new() { Id=Guid.NewGuid(), Nombre="Codo PVC 3/4\" 90°",               TipoMaterial="Plomería",          Descripcion="Codo PVC presión 90 grados de 3/4 pulgada",                  PrecioUnitario=1800,   StockActual=150 },
            new() { Id=Guid.NewGuid(), Nombre="Tee PVC 1/2\"",                    TipoMaterial="Plomería",          Descripcion="Tee PVC presión de 1/2 pulgada",                              PrecioUnitario=1500,   StockActual=150 },
            new() { Id=Guid.NewGuid(), Nombre="Unión PVC 1/2\"",                   TipoMaterial="Plomería",          Descripcion="Unión de reparación PVC 1/2 pulgada",                         PrecioUnitario=900,    StockActual=200 },
            new() { Id=Guid.NewGuid(), Nombre="Sifón lavamanos PVC",               TipoMaterial="Plomería",          Descripcion="Sifón botella PVC para lavamanos 1 1/4\"",                   PrecioUnitario=8500,   StockActual=25 },
            new() { Id=Guid.NewGuid(), Nombre="Empaque de lavamanos",              TipoMaterial="Plomería",          Descripcion="Kit de empaques de caucho para llave de lavamanos",           PrecioUnitario=3500,   StockActual=80 },
            new() { Id=Guid.NewGuid(), Nombre="Empaque de llave 1/2\"",            TipoMaterial="Plomería",          Descripcion="Empaque de caucho para llave de paso 1/2 pulgada",           PrecioUnitario=800,    StockActual=120 },
            new() { Id=Guid.NewGuid(), Nombre="Cinta teflón 1/2\"",                TipoMaterial="Plomería",          Descripcion="Cinta teflón PTFE blanca 12mm x 10m",                         PrecioUnitario=1500,   StockActual=200 },
            new() { Id=Guid.NewGuid(), Nombre="Silicona para baño blanca",         TipoMaterial="Selladores",        Descripcion="Silicona sanitaria blanca fungicida 280ml",                   PrecioUnitario=18000,  StockActual=40 },
            new() { Id=Guid.NewGuid(), Nombre="Soldadura PVC 250ml",               TipoMaterial="Plomería",          Descripcion="Pegamento para PVC presión y sanitario 250ml",               PrecioUnitario=22000,  StockActual=30 },

            // ── Eléctrico ─────────────────────────────────────────────────
            new() { Id=Guid.NewGuid(), Nombre="Cable eléctrico #12 AWG rojo (m)",  TipoMaterial="Eléctrico",         Descripcion="Cable THHN/THWN #12 AWG color rojo, venta por metro",        PrecioUnitario=2800,   StockActual=500 },
            new() { Id=Guid.NewGuid(), Nombre="Cable eléctrico #12 AWG negro (m)", TipoMaterial="Eléctrico",         Descripcion="Cable THHN/THWN #12 AWG color negro, venta por metro",       PrecioUnitario=2800,   StockActual=500 },
            new() { Id=Guid.NewGuid(), Nombre="Cable eléctrico #14 AWG (m)",       TipoMaterial="Eléctrico",         Descripcion="Cable THHN/THWN #14 AWG, venta por metro",                   PrecioUnitario=2200,   StockActual=400 },
            new() { Id=Guid.NewGuid(), Nombre="Cable tierra #12 AWG (m)",          TipoMaterial="Eléctrico",         Descripcion="Cable desnudo o verde tierra #12 AWG, por metro",            PrecioUnitario=2500,   StockActual=300 },
            new() { Id=Guid.NewGuid(), Nombre="Interruptor sencillo",              TipoMaterial="Eléctrico",         Descripcion="Interruptor de luz sencillo 15A 120V",                        PrecioUnitario=12000,  StockActual=50 },
            new() { Id=Guid.NewGuid(), Nombre="Interruptor doble",                 TipoMaterial="Eléctrico",         Descripcion="Interruptor de luz doble 15A 120V",                           PrecioUnitario=18000,  StockActual=35 },
            new() { Id=Guid.NewGuid(), Nombre="Toma corriente doble",              TipoMaterial="Eléctrico",         Descripcion="Tomacorriente doble polarizado 15A 125V",                     PrecioUnitario=10000,  StockActual=60 },
            new() { Id=Guid.NewGuid(), Nombre="Toma corriente GFCI",               TipoMaterial="Eléctrico",         Descripcion="Tomacorriente GFCI con protección para zonas húmedas 20A",  PrecioUnitario=45000,  StockActual=20 },
            new() { Id=Guid.NewGuid(), Nombre="Breaker 15A monofásico",            TipoMaterial="Eléctrico",         Descripcion="Interruptor termomagnético monofásico 15A 120V",              PrecioUnitario=28000,  StockActual=25 },
            new() { Id=Guid.NewGuid(), Nombre="Breaker 20A monofásico",            TipoMaterial="Eléctrico",         Descripcion="Interruptor termomagnético monofásico 20A 120V",              PrecioUnitario=32000,  StockActual=20 },
            new() { Id=Guid.NewGuid(), Nombre="Breaker 30A bifásico",              TipoMaterial="Eléctrico",         Descripcion="Interruptor termomagnético bifásico 30A 240V",                PrecioUnitario=55000,  StockActual=15 },
            new() { Id=Guid.NewGuid(), Nombre="Canaleta plástica 20x12mm (m)",     TipoMaterial="Eléctrico",         Descripcion="Canaleta ranurada PVC para cableado 20x12mm por metro",      PrecioUnitario=6500,   StockActual=100 },
            new() { Id=Guid.NewGuid(), Nombre="Caja eléctrica metálica 4\"x4\"",   TipoMaterial="Eléctrico",         Descripcion="Caja de salida metálica 4x4 pulgadas",                        PrecioUnitario=8500,   StockActual=40 },
            new() { Id=Guid.NewGuid(), Nombre="Caja eléctrica PVC 2\"x4\"",        TipoMaterial="Eléctrico",         Descripcion="Caja de salida PVC 2x4 pulgadas para empotrar",               PrecioUnitario=4500,   StockActual=60 },
            new() { Id=Guid.NewGuid(), Nombre="Tubo conduit 3/4\" EMT (3m)",       TipoMaterial="Eléctrico",         Descripcion="Tubo conduit metálico EMT 3/4 pulgada longitud 3m",          PrecioUnitario=35000,  StockActual=30 },
            new() { Id=Guid.NewGuid(), Nombre="Conector conduit 3/4\"",            TipoMaterial="Eléctrico",         Descripcion="Conector recto para conduit EMT 3/4 pulgada",                 PrecioUnitario=3200,   StockActual=80 },
            new() { Id=Guid.NewGuid(), Nombre="Cinta aislante negra",              TipoMaterial="Eléctrico",         Descripcion="Cinta aislante PVC 19mm x 20m negro 600V",                   PrecioUnitario=4500,   StockActual=80 },
            new() { Id=Guid.NewGuid(), Nombre="Cinta aislante roja",               TipoMaterial="Eléctrico",         Descripcion="Cinta aislante PVC 19mm x 20m rojo 600V",                    PrecioUnitario=4500,   StockActual=60 },
            new() { Id=Guid.NewGuid(), Nombre="Marquillas para cable (paq 100)",   TipoMaterial="Eléctrico",         Descripcion="Marquillas de identificación para cables, paquete 100 und",  PrecioUnitario=8000,   StockActual=30 },
            new() { Id=Guid.NewGuid(), Nombre="Prensaestopa PG-11",                TipoMaterial="Eléctrico",         Descripcion="Prensaestopa plástico PG-11 para cable 5-10mm",               PrecioUnitario=2500,   StockActual=60 },

            // ── Pintura y acabados ────────────────────────────────────────
            new() { Id=Guid.NewGuid(), Nombre="Pintura blanca de caucho 1 galón",  TipoMaterial="Pintura",           Descripcion="Pintura de caucho interior/exterior blanca 1 galón",         PrecioUnitario=68000,  StockActual=20 },
            new() { Id=Guid.NewGuid(), Nombre="Pintura beige de caucho 1 galón",   TipoMaterial="Pintura",           Descripcion="Pintura de caucho interior/exterior beige 1 galón",          PrecioUnitario=68000,  StockActual=15 },
            new() { Id=Guid.NewGuid(), Nombre="Pintura blanca vinilo 1 galón",     TipoMaterial="Pintura",           Descripcion="Pintura vinílica interior blanca lavable 1 galón",           PrecioUnitario=75000,  StockActual=15 },
            new() { Id=Guid.NewGuid(), Nombre="Pintura esmalte blanco 1 galón",    TipoMaterial="Pintura",           Descripcion="Pintura esmalte alkídico blanco brillante 1 galón",          PrecioUnitario=82000,  StockActual=12 },
            new() { Id=Guid.NewGuid(), Nombre="Pintura anticorrosivo gris 1 galón",TipoMaterial="Pintura",           Descripcion="Pintura anticorrosiva gris para metales 1 galón",            PrecioUnitario=88000,  StockActual=10 },
            new() { Id=Guid.NewGuid(), Nombre="Rodillo de pintura 9\"",            TipoMaterial="Pintura",           Descripcion="Rodillo de espuma para pintura 9 pulgadas con mango",        PrecioUnitario=12000,  StockActual=30 },
            new() { Id=Guid.NewGuid(), Nombre="Brocha 2\"",                        TipoMaterial="Pintura",           Descripcion="Brocha de cerdas naturales 2 pulgadas",                       PrecioUnitario=8500,   StockActual=40 },
            new() { Id=Guid.NewGuid(), Nombre="Brocha 4\"",                        TipoMaterial="Pintura",           Descripcion="Brocha de cerdas naturales 4 pulgadas",                       PrecioUnitario=14000,  StockActual=30 },
            new() { Id=Guid.NewGuid(), Nombre="Sellador de paredes 1 galón",       TipoMaterial="Pintura",           Descripcion="Sellador fijador acrílico transparente para paredes 1 galón",PrecioUnitario=58000,  StockActual=15 },
            new() { Id=Guid.NewGuid(), Nombre="Lija de agua #120",                 TipoMaterial="Pintura",           Descripcion="Lija de agua grano 120, pliego 230x280mm",                   PrecioUnitario=2200,   StockActual=100 },
            new() { Id=Guid.NewGuid(), Nombre="Lija de agua #220",                 TipoMaterial="Pintura",           Descripcion="Lija de agua grano 220, pliego 230x280mm",                   PrecioUnitario=2200,   StockActual=100 },
            new() { Id=Guid.NewGuid(), Nombre="Lija para pared #80",               TipoMaterial="Pintura",           Descripcion="Lija seca para pared grano 80, pliego",                       PrecioUnitario=1800,   StockActual=100 },
            new() { Id=Guid.NewGuid(), Nombre="Masilla para pared 1 kg",           TipoMaterial="Pintura",           Descripcion="Masilla corriente para pared y cielo raso, 1 kg",            PrecioUnitario=12000,  StockActual=40 },
            new() { Id=Guid.NewGuid(), Nombre="Estuco plástico 25 kg",             TipoMaterial="Pintura",           Descripcion="Estuco plástico en polvo para paredes interiores, bulto 25kg",PrecioUnitario=65000, StockActual=20 },
            new() { Id=Guid.NewGuid(), Nombre="Thinner acrílico 1 litro",          TipoMaterial="Pintura",           Descripcion="Disolvente acrílico para dilución de pinturas base agua 1L", PrecioUnitario=18000,  StockActual=30 },
            new() { Id=Guid.NewGuid(), Nombre="Cinta de enmascarar 1\"",           TipoMaterial="Pintura",           Descripcion="Cinta de enmascarar crepe 24mm x 50m",                        PrecioUnitario=6500,   StockActual=60 },
            new() { Id=Guid.NewGuid(), Nombre="Cinta de enmascarar 2\"",           TipoMaterial="Pintura",           Descripcion="Cinta de enmascarar crepe 48mm x 50m",                        PrecioUnitario=10000,  StockActual=40 },

            // ── Fijaciones ────────────────────────────────────────────────
            new() { Id=Guid.NewGuid(), Nombre="Tornillo pared 3/8\"x2\" (x100)",   TipoMaterial="Fijaciones",        Descripcion="Tornillo para pared 3/8 x 2 pulgadas, caja x 100 unidades", PrecioUnitario=15000,  StockActual=30 },
            new() { Id=Guid.NewGuid(), Nombre="Tornillo madera #8x1.5\" (x100)",   TipoMaterial="Fijaciones",        Descripcion="Tornillo para madera cabeza plana #8 x 1.5 pulgadas x 100", PrecioUnitario=12000,  StockActual=30 },
            new() { Id=Guid.NewGuid(), Nombre="Tornillo autoperforante #10x1\" (x50)",TipoMaterial="Fijaciones",     Descripcion="Tornillo autoperfante #10 x 1 pulgada, caja x 50 und",      PrecioUnitario=9000,   StockActual=40 },
            new() { Id=Guid.NewGuid(), Nombre="Taco fisher #6 (x100)",             TipoMaterial="Fijaciones",        Descripcion="Taco de expansión nylon #6, caja x 100 unidades",             PrecioUnitario=8000,   StockActual=50 },
            new() { Id=Guid.NewGuid(), Nombre="Taco fisher #8 (x100)",             TipoMaterial="Fijaciones",        Descripcion="Taco de expansión nylon #8, caja x 100 unidades",             PrecioUnitario=10000,  StockActual=40 },
            new() { Id=Guid.NewGuid(), Nombre="Taco fisher #10 (x50)",             TipoMaterial="Fijaciones",        Descripcion="Taco de expansión nylon #10, caja x 50 unidades",             PrecioUnitario=8500,   StockActual=35 },
            new() { Id=Guid.NewGuid(), Nombre="Puntilla 2\" (lb)",                 TipoMaterial="Fijaciones",        Descripcion="Puntilla de acero brillante 2 pulgadas, por libra",          PrecioUnitario=4500,   StockActual=30 },
            new() { Id=Guid.NewGuid(), Nombre="Puntilla 3\" (lb)",                 TipoMaterial="Fijaciones",        Descripcion="Puntilla de acero brillante 3 pulgadas, por libra",          PrecioUnitario=4500,   StockActual=25 },
            new() { Id=Guid.NewGuid(), Nombre="Tornillo cabeza plana M5x20mm",     TipoMaterial="Fijaciones",        Descripcion="Tornillo métrico cabeza plana M5 x 20mm acero",               PrecioUnitario=500,    StockActual=300 },
            new() { Id=Guid.NewGuid(), Nombre="Tuerca M5 (x50)",                   TipoMaterial="Fijaciones",        Descripcion="Tuerca hexagonal métrica M5 acero galvanizado, x 50",        PrecioUnitario=4000,   StockActual=100 },
            new() { Id=Guid.NewGuid(), Nombre="Arandela plana M5 (x50)",           TipoMaterial="Fijaciones",        Descripcion="Arandela plana M5 acero galvanizado, paquete x 50",          PrecioUnitario=3000,   StockActual=100 },
            new() { Id=Guid.NewGuid(), Nombre="Ancla química 330ml",               TipoMaterial="Fijaciones",        Descripcion="Mortero de anclaje epóxico bicomponente 330ml",               PrecioUnitario=85000,  StockActual=15 },
            new() { Id=Guid.NewGuid(), Nombre="Platina acero 1\"x1/8\" (m)",       TipoMaterial="Fijaciones",        Descripcion="Platina laminada en frío 1 x 1/8 pulgadas, por metro",       PrecioUnitario=12000,  StockActual=20 },
            new() { Id=Guid.NewGuid(), Nombre="Perfil ángulo 1\"x1/8\" (m)",       TipoMaterial="Fijaciones",        Descripcion="Ángulo de acero laminado 1 x 1/8 pulgadas, por metro",       PrecioUnitario=14000,  StockActual=20 },

            // ── Selladores y adhesivos ────────────────────────────────────
            new() { Id=Guid.NewGuid(), Nombre="Pegamento de contacto 500ml",       TipoMaterial="Selladores",        Descripcion="Pegamento de contacto universal 500ml",                       PrecioUnitario=28000,  StockActual=20 },
            new() { Id=Guid.NewGuid(), Nombre="Pegamento epoxi bicomponente",      TipoMaterial="Selladores",        Descripcion="Epóxico bicomponente A+B alta resistencia 200ml",            PrecioUnitario=35000,  StockActual=15 },
            new() { Id=Guid.NewGuid(), Nombre="Silicona de construcción gris",     TipoMaterial="Selladores",        Descripcion="Silicona neutra gris para juntas de construcción 280ml",     PrecioUnitario=16000,  StockActual=30 },
            new() { Id=Guid.NewGuid(), Nombre="Espuma de poliuretano 500ml",       TipoMaterial="Selladores",        Descripcion="Espuma expansiva de poliuretano selladora 500ml",            PrecioUnitario=32000,  StockActual=20 },
            new() { Id=Guid.NewGuid(), Nombre="Bondex impermeabilizante 1 galón",  TipoMaterial="Selladores",        Descripcion="Impermeabilizante cementicio elástico 1 galón",               PrecioUnitario=95000,  StockActual=12 },
            new() { Id=Guid.NewGuid(), Nombre="Impermeabilizante acrílico 1 galón",TipoMaterial="Selladores",        Descripcion="Impermeabilizante acrílico en emulsión 1 galón",             PrecioUnitario=88000,  StockActual=10 },
            new() { Id=Guid.NewGuid(), Nombre="Sellador de grietas 300ml",         TipoMaterial="Selladores",        Descripcion="Sellador acrílico flexible para grietas y juntas 300ml",     PrecioUnitario=18000,  StockActual=25 },

            // ── Limpieza y seguridad ──────────────────────────────────────
            new() { Id=Guid.NewGuid(), Nombre="Desengrasante industrial 1 litro",  TipoMaterial="Limpieza",          Descripcion="Desengrasante concentrado para superficies metálicas 1L",   PrecioUnitario=22000,  StockActual=20 },
            new() { Id=Guid.NewGuid(), Nombre="Limpiador multiusos 1 litro",       TipoMaterial="Limpieza",          Descripcion="Limpiador multiusos en spray para mantenimiento general 1L", PrecioUnitario=15000,  StockActual=30 },
            new() { Id=Guid.NewGuid(), Nombre="Removedor de óxido 500ml",          TipoMaterial="Limpieza",          Descripcion="Removedor y convertidor de óxido para metales 500ml",        PrecioUnitario=28000,  StockActual=15 },
            new() { Id=Guid.NewGuid(), Nombre="Lubricante en spray 400ml",         TipoMaterial="Limpieza",          Descripcion="Lubricante multiusos WD tipo en aerosol 400ml",              PrecioUnitario=22000,  StockActual=25 },
            new() { Id=Guid.NewGuid(), Nombre="Guantes de caucho talla M",         TipoMaterial="Seguridad",         Descripcion="Guantes industriales de caucho resistentes, talla M",        PrecioUnitario=12000,  StockActual=40 },
            new() { Id=Guid.NewGuid(), Nombre="Guantes de caucho talla L",         TipoMaterial="Seguridad",         Descripcion="Guantes industriales de caucho resistentes, talla L",        PrecioUnitario=12000,  StockActual=40 },
            new() { Id=Guid.NewGuid(), Nombre="Tapabocas N95 (x10)",               TipoMaterial="Seguridad",         Descripcion="Respirador N95 con válvula de exhalación, caja x 10",        PrecioUnitario=45000,  StockActual=20 },
            new() { Id=Guid.NewGuid(), Nombre="Gafas de seguridad transparentes",  TipoMaterial="Seguridad",         Descripcion="Lentes de seguridad con protección UV y antiempañante",      PrecioUnitario=18000,  StockActual=20 },
            new() { Id=Guid.NewGuid(), Nombre="Casco de seguridad blanco",         TipoMaterial="Seguridad",         Descripcion="Casco de protección HDPE clase E blanco con ajuste",         PrecioUnitario=55000,  StockActual=10 },
            new() { Id=Guid.NewGuid(), Nombre="Arnés de seguridad 2 puntos",       TipoMaterial="Seguridad",         Descripcion="Arnés anticaída de cuerpo completo 2 puntos de anclaje",     PrecioUnitario=280000, StockActual=5 },

            // ── Herramientas y consumibles ────────────────────────────────
            new() { Id=Guid.NewGuid(), Nombre="Disco de corte 4.5\" metal",        TipoMaterial="Herramientas",      Descripcion="Disco abrasivo de corte para metales 4.5 pulgadas",          PrecioUnitario=8500,   StockActual=40 },
            new() { Id=Guid.NewGuid(), Nombre="Disco de desbaste 4.5\"",           TipoMaterial="Herramientas",      Descripcion="Disco abrasivo de desbaste 4.5 pulgadas para metales",       PrecioUnitario=9000,   StockActual=30 },
            new() { Id=Guid.NewGuid(), Nombre="Disco de corte 4.5\" mampostería",  TipoMaterial="Herramientas",      Descripcion="Disco diamantado de corte para concreto 4.5 pulgadas",       PrecioUnitario=45000,  StockActual=10 },
            new() { Id=Guid.NewGuid(), Nombre="Broca para concreto 3/8\"",         TipoMaterial="Herramientas",      Descripcion="Broca SDS para concreto y mampostería 3/8 pulgada",          PrecioUnitario=12000,  StockActual=20 },
            new() { Id=Guid.NewGuid(), Nombre="Broca para concreto 1/2\"",         TipoMaterial="Herramientas",      Descripcion="Broca SDS para concreto y mampostería 1/2 pulgada",          PrecioUnitario=18000,  StockActual=15 },
            new() { Id=Guid.NewGuid(), Nombre="Broca para metal 3/8\"",            TipoMaterial="Herramientas",      Descripcion="Broca HSS para metal 3/8 pulgada",                            PrecioUnitario=8500,   StockActual=20 },
            new() { Id=Guid.NewGuid(), Nombre="Electrodo 6013 3/32\" (kg)",        TipoMaterial="Herramientas",      Descripcion="Electrodo para soldadura 6013 diámetro 3/32 pulgada por kg", PrecioUnitario=28000,  StockActual=15 },
            new() { Id=Guid.NewGuid(), Nombre="Sierra segueta bimetálica (x5)",    TipoMaterial="Herramientas",      Descripcion="Hoja de segueta bimetálica 12 pulgadas, paquete x 5",        PrecioUnitario=22000,  StockActual=15 },
            new() { Id=Guid.NewGuid(), Nombre="Lija de banda #80",                 TipoMaterial="Herramientas",      Descripcion="Banda de lija #80 para lijadora de banda 3x21 pulgadas",     PrecioUnitario=8500,   StockActual=20 },
            new() { Id=Guid.NewGuid(), Nombre="Broca plana para madera 1\"",       TipoMaterial="Herramientas",      Descripcion="Broca plana de paleta para madera 1 pulgada",                 PrecioUnitario=9500,   StockActual=10 },

            // ── Cerrajería ────────────────────────────────────────────────
            new() { Id=Guid.NewGuid(), Nombre="Cerradura cilindro seguridad",      TipoMaterial="Cerrajería",        Descripcion="Cerradura de pomo con cilindro de seguridad doble",          PrecioUnitario=125000, StockActual=10 },
            new() { Id=Guid.NewGuid(), Nombre="Cerradura de palanca interior",     TipoMaterial="Cerrajería",        Descripcion="Cerradura de palanca para uso interior baño/alcoba",         PrecioUnitario=85000,  StockActual=12 },
            new() { Id=Guid.NewGuid(), Nombre="Bisagra acero 3\" (par)",           TipoMaterial="Cerrajería",        Descripcion="Bisagra de acero inoxidable 3 pulgadas, par",                 PrecioUnitario=15000,  StockActual=30 },
            new() { Id=Guid.NewGuid(), Nombre="Bisagra acero 4\" (par)",           TipoMaterial="Cerrajería",        Descripcion="Bisagra de acero inoxidable 4 pulgadas, par",                 PrecioUnitario=20000,  StockActual=20 },
            new() { Id=Guid.NewGuid(), Nombre="Manija de puerta acero inox",       TipoMaterial="Cerrajería",        Descripcion="Manija de puerta en acero inoxidable cepillado",              PrecioUnitario=65000,  StockActual=15 },
            new() { Id=Guid.NewGuid(), Nombre="Candado 40mm con llave",            TipoMaterial="Cerrajería",        Descripcion="Candado de arco 40mm con 2 llaves incluidas",                 PrecioUnitario=28000,  StockActual=15 },
            new() { Id=Guid.NewGuid(), Nombre="Aldaba con candado 3\"",            TipoMaterial="Cerrajería",        Descripcion="Aldaba pasador con ojal para candado, 3 pulgadas",            PrecioUnitario=12000,  StockActual=20 },

            // ── Drywall y cielo raso ──────────────────────────────────────
            new() { Id=Guid.NewGuid(), Nombre="Lámina drywall 1/2\" (1.22x2.44m)",TipoMaterial="Drywall",           Descripcion="Lámina de yeso drywall estándar 1/2 pulgada",                 PrecioUnitario=38000,  StockActual=20 },
            new() { Id=Guid.NewGuid(), Nombre="Canal de carga 3 5/8\" (3m)",      TipoMaterial="Drywall",           Descripcion="Perfil canal metálico galvanizado 3 5/8 pulgadas x 3m",      PrecioUnitario=22000,  StockActual=25 },
            new() { Id=Guid.NewGuid(), Nombre="Parante 3 5/8\" (3m)",             TipoMaterial="Drywall",           Descripcion="Perfil parante metálico galvanizado 3 5/8 pulgadas x 3m",    PrecioUnitario=25000,  StockActual=25 },
            new() { Id=Guid.NewGuid(), Nombre="Cinta de papel para drywall (75m)", TipoMaterial="Drywall",           Descripcion="Cinta de papel perforado para juntas de drywall 75m",        PrecioUnitario=12000,  StockActual=20 },
            new() { Id=Guid.NewGuid(), Nombre="Masilla lista para drywall 28 kg",  TipoMaterial="Drywall",           Descripcion="Masilla preapareada para acabados drywall, cubo 28kg",        PrecioUnitario=95000,  StockActual=10 },
        };

        await context.Set<Material>().AddRangeAsync(materiales);
        await context.SaveChangesAsync();
        Console.WriteLine($"[MaterialSeeder] {materiales.Count} materiales cargados.");
    }
}
