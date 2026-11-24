SELECT fp.name AS plan_name,
       r.name  AS room_name,
       COUNT(po.id) AS furniture_count,
       ROUND(SUM(po.width * po.depth)::numeric, 2)::float8 AS total_footprint,
       RANK() OVER (PARTITION BY fp.id ORDER BY SUM(po.width * po.depth) DESC) AS rank_in_plan
FROM floor_plans fp
JOIN rooms r ON r.floor_plan_id = fp.id
JOIN plan_objects po ON po.room_id = r.id
WHERE po.kind = 'furniture'
GROUP BY fp.id, fp.name, r.id, r.name
HAVING SUM(po.width * po.depth) >= 0.5
ORDER BY fp.name, rank_in_plan, r.name;
